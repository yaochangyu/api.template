# 多層快取策略詳解

## 概述

本專案採用 **L1 (Memory Cache) + L2 (Redis)** 的混合快取架構，在高效能與一致性之間取得平衡。

## 架構設計

```
請求 → L1 快取（記憶體）→ L2 快取（Redis）→ 資料庫
        ↑ 5-10 分鐘            ↑ 15-60 分鐘       ↑ 永久儲存
        微秒級回應             毫秒級回應          秒級回應
```

### 快取層級特性對比

| 特性 | L1 (Memory Cache) | L2 (Redis) | 資料庫 |
|------|-------------------|------------|--------|
| 存取速度 | 微秒級 (~1-10 μs) | 毫秒級 (~1-5 ms) | 秒級 (~10-100 ms) |
| 容量 | 有限 (MB-GB) | 大 (GB-TB) | 極大 (TB-PB) |
| 持久性 | ❌ 程序重啟消失 | ✅ 可設定持久化 | ✅ 永久儲存 |
| 分散式 | ❌ 單機 | ✅ 跨伺服器共用 | ✅ 跨伺服器 |
| 適用場景 | 熱點資料 | 共用資料、Session | 業務資料 |

## 實作範例

### 1. 基礎設定

```csharp
// appsettings.json
{
  "Cache": {
    "Redis": {
      "Connection": "localhost:6379",
      "InstanceName": "JobBank:"
    },
    "Memory": {
      "SizeLimit": 1024,  // MB
      "CompactionPercentage": 0.25
    },
    "DefaultExpiration": {
      "L1Minutes": 5,
      "L2Minutes": 15
    }
  }
}

// Program.cs 註冊
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024 * 1024 * 1024; // 1 GB
    options.CompactionPercentage = 0.25;
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Cache:Redis:Connection"];
    options.InstanceName = builder.Configuration["Cache:Redis:InstanceName"];
});

builder.Services.AddSingleton<ICacheService, HybridCacheService>();
```

### 2. 混合快取服務實作

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}

public sealed class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly TimeSpan _defaultL1Expiration;
    private readonly TimeSpan _defaultL2Expiration;
    
    public HybridCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        IConfiguration configuration,
        ILogger<HybridCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        
        _defaultL1Expiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Cache:DefaultExpiration:L1Minutes", 5));
        _defaultL2Expiration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("Cache:DefaultExpiration:L2Minutes", 15));
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        // 1. 嘗試從 L1 取得
        if (_memoryCache.TryGetValue<T>(key, out var l1Value))
        {
            _logger.LogDebug("L1 Cache hit: {Key}", key);
            return l1Value;
        }
        
        // 2. L1 未命中，嘗試從 L2 取得
        var l2Json = await _distributedCache.GetStringAsync(key, ct);
        if (l2Json == null)
        {
            _logger.LogDebug("L2 Cache miss: {Key}", key);
            return default;
        }
        
        _logger.LogDebug("L2 Cache hit: {Key}", key);
        
        // 3. 反序列化
        var l2Value = JsonSerializer.Deserialize<T>(l2Json);
        
        // 4. 回寫到 L1（較短的過期時間）
        var l1Options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultL1Expiration,
            Size = 1 // 用於 Size Limit 計算
        };
        _memoryCache.Set(key, l2Value, l1Options);
        
        return l2Value;
    }
    
    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null, 
        CancellationToken ct = default)
    {
        var l2Expiration = expiration ?? _defaultL2Expiration;
        var l1Expiration = TimeSpan.FromTicks(Math.Min(
            _defaultL1Expiration.Ticks, 
            l2Expiration.Ticks));
        
        // 1. 寫入 L2 (Redis)
        var json = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = l2Expiration
        };
        await _distributedCache.SetStringAsync(key, json, options, ct);
        
        // 2. 同步寫入 L1 (Memory)
        var l1Options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = l1Expiration,
            Size = 1
        };
        _memoryCache.Set(key, value, l1Options);
        
        _logger.LogDebug("Cache set: {Key}, L1: {L1Exp}min, L2: {L2Exp}min", 
            key, l1Expiration.TotalMinutes, l2Expiration.TotalMinutes);
    }
    
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        // 1. 移除 L1
        _memoryCache.Remove(key);
        
        // 2. 移除 L2
        await _distributedCache.RemoveAsync(key, ct);
        
        _logger.LogDebug("Cache removed: {Key}", key);
    }
    
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        // ⚠️ Redis 批次刪除需要使用 StackExchange.Redis 直接操作
        // IDistributedCache 介面不支援 Key 掃描
        
        _logger.LogWarning("RemoveByPrefix not fully implemented for L1+L2");
        // 實作略（需要 IConnectionMultiplexer）
    }
}
```

### 3. Cache-Aside Pattern（旁路快取模式）

```csharp
public sealed class MemberHandler
{
    private readonly MemberRepository _repository;
    private readonly ICacheService _cache;
    
    public async Task<Result<Member>> GetMemberAsync(
        string id, 
        CancellationToken ct = default)
    {
        var cacheKey = $"member:{id}";
        
        // 1. 嘗試從快取取得
        var cached = await _cache.GetAsync<Member>(cacheKey, ct);
        if (cached != null)
            return Result.Success(cached);
        
        // 2. 快取未命中，從資料庫查詢
        var member = await _repository.GetByIdAsync(id, ct);
        if (member == null)
            return Result.Failure<Member>("會員不存在", "NOT_FOUND");
        
        // 3. 寫入快取
        await _cache.SetAsync(cacheKey, member, TimeSpan.FromMinutes(15), ct);
        
        return Result.Success(member);
    }
}
```

### 4. Write-Through Pattern（寫穿模式）

```csharp
public async Task<Result<Member>> UpdateMemberAsync(
    string id,
    UpdateMemberRequest request,
    CancellationToken ct = default)
{
    // 1. 更新資料庫
    var member = await _repository.UpdateAsync(id, request, ct);
    
    // 2. 同步更新快取
    var cacheKey = $"member:{id}";
    await _cache.SetAsync(cacheKey, member, TimeSpan.FromMinutes(15), ct);
    
    // 3. 清除相關快取（例如：列表快取）
    await _cache.RemoveByPrefixAsync("member:list:", ct);
    
    return Result.Success(member);
}
```

### 5. Cache Invalidation（快取失效策略）

```csharp
public sealed class MemberEventHandler
{
    private readonly ICacheService _cache;
    
    // 會員更新時清除快取
    public async Task OnMemberUpdatedAsync(string memberId)
    {
        await _cache.RemoveAsync($"member:{memberId}");
        await _cache.RemoveByPrefixAsync("member:list:");  // 清除列表快取
    }
    
    // 會員刪除時清除快取
    public async Task OnMemberDeletedAsync(string memberId)
    {
        await _cache.RemoveAsync($"member:{memberId}");
        await _cache.RemoveByPrefixAsync("member:list:");
    }
}
```

## 進階模式

### 1. Stale-While-Revalidate（提供過期資料並背景更新）

```csharp
public async Task<T?> GetWithRevalidationAsync<T>(
    string key,
    Func<Task<T>> fetchFunc,
    TimeSpan expiration,
    CancellationToken ct = default)
{
    var cached = await GetAsync<T>(key, ct);
    if (cached != null)
    {
        // 檢查是否接近過期（例如：剩餘 20% 時間）
        var metadata = _memoryCache.Get<CacheMetadata>($"{key}:meta");
        if (metadata?.IsNearExpiration() == true)
        {
            // 背景更新（不等待）
            _ = Task.Run(async () =>
            {
                var fresh = await fetchFunc();
                await SetAsync(key, fresh, expiration);
            }, ct);
        }
        
        return cached;
    }
    
    // 未命中，同步載入
    var value = await fetchFunc();
    await SetAsync(key, value, expiration, ct);
    return value;
}
```

### 2. 防止快取穿透（Null Object Pattern）

```csharp
private static readonly Member NullMember = new() { Id = "NULL" };

public async Task<Result<Member>> GetMemberAsync(string id, CancellationToken ct)
{
    var cacheKey = $"member:{id}";
    var cached = await _cache.GetAsync<Member>(cacheKey, ct);
    
    // 檢查是否為 Null Object
    if (cached != null)
    {
        return cached.Id == "NULL" 
            ? Result.Failure<Member>("會員不存在", "NOT_FOUND")
            : Result.Success(cached);
    }
    
    var member = await _repository.GetByIdAsync(id, ct);
    
    // 即使查不到，也快取 Null Object（較短過期時間）
    await _cache.SetAsync(
        cacheKey, 
        member ?? NullMember, 
        member != null ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(2), 
        ct);
    
    return member != null 
        ? Result.Success(member) 
        : Result.Failure<Member>("會員不存在", "NOT_FOUND");
}
```

### 3. 分散式鎖（防止快取擊穿）

```csharp
public async Task<T> GetOrSetAsync<T>(
    string key,
    Func<Task<T>> fetchFunc,
    TimeSpan expiration,
    CancellationToken ct = default)
{
    // 1. 嘗試從快取取得
    var cached = await GetAsync<T>(key, ct);
    if (cached != null) return cached;
    
    // 2. 使用分散式鎖（RedLock）
    var lockKey = $"lock:{key}";
    var redLock = await AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10), ct);
    
    try
    {
        // 3. Double-Check（可能其他執行緒已載入）
        cached = await GetAsync<T>(key, ct);
        if (cached != null) return cached;
        
        // 4. 載入資料
        var value = await fetchFunc();
        
        // 5. 寫入快取
        await SetAsync(key, value, expiration, ct);
        
        return value;
    }
    finally
    {
        await redLock.ReleaseAsync();
    }
}

private async Task<IRedLock> AcquireLockAsync(
    string key, 
    TimeSpan expiry, 
    CancellationToken ct)
{
    // 使用 RedLock.net 或自行實作
    var redis = /* IConnectionMultiplexer */;
    var db = redis.GetDatabase();
    
    var lockId = Guid.NewGuid().ToString("N");
    var acquired = await db.StringSetAsync(
        key, 
        lockId, 
        expiry, 
        When.NotExists);
    
    if (!acquired)
        throw new InvalidOperationException("無法取得鎖");
    
    return new RedisLock(db, key, lockId);
}
```

## 快取鍵命名規範

```csharp
// 格式：{實體類型}:{唯一識別碼}
"member:12345"
"order:ORD-2024-001"

// 列表快取：{實體類型}:list:{篩選條件}
"member:list:active"
"order:list:pending:page:1"

// 統計快取：{實體類型}:stats:{類型}
"member:stats:count"
"order:stats:daily:2024-01-01"

// 關聯快取：{實體類型}:{ID}:{關聯實體}
"member:12345:orders"
"order:ORD-001:items"
```

## 效能監控

### 1. 快取命中率追蹤

```csharp
public sealed class CacheMetrics
{
    private long _l1Hits;
    private long _l2Hits;
    private long _misses;
    
    public void RecordL1Hit() => Interlocked.Increment(ref _l1Hits);
    public void RecordL2Hit() => Interlocked.Increment(ref _l2Hits);
    public void RecordMiss() => Interlocked.Increment(ref _misses);
    
    public double L1HitRate => (double)_l1Hits / (_l1Hits + _l2Hits + _misses);
    public double L2HitRate => (double)_l2Hits / (_l2Hits + _misses);
    public double OverallHitRate => (double)(_l1Hits + _l2Hits) / (_l1Hits + _l2Hits + _misses);
}
```

### 2. Serilog 整合

```csharp
_logger.LogInformation(
    "Cache operation: {Operation}, Key: {Key}, Hit: {Hit}, Duration: {DurationMs}ms",
    "Get",
    key,
    cached != null,
    stopwatch.ElapsedMilliseconds);
```

## 測試策略

### 整合測試（使用 Testcontainers Redis）

```csharp
public class CacheIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;
    private ICacheService _cacheService = null!;
    
    public CacheIntegrationTests()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
        
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = _redisContainer.GetConnectionString();
        });
        services.AddSingleton<ICacheService, HybridCacheService>();
        
        var provider = services.BuildServiceProvider();
        _cacheService = provider.GetRequiredService<ICacheService>();
    }
    
    [Fact]
    public async Task Get_ShouldReturnCachedValue_WhenExists()
    {
        // Arrange
        var key = "test:key";
        var value = new TestData { Id = 123, Name = "Test" };
        await _cacheService.SetAsync(key, value);
        
        // Act
        var result = await _cacheService.GetAsync<TestData>(key);
        
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
    }
    
    public async Task DisposeAsync()
    {
        await _redisContainer.StopAsync();
    }
}
```

## 常見陷阱

- ❌ **快取雪崩**：大量快取同時過期 → 使用隨機過期時間
- ❌ **快取穿透**：查詢不存在的資料 → 快取 Null Object
- ❌ **快取擊穿**：熱點資料過期瞬間 → 使用分散式鎖
- ❌ **記憶體洩漏**：L1 快取無限增長 → 設定 SizeLimit
- ❌ **序列化問題**：非 JSON 友善的類型 → 使用 DTO

## 參考資料

- [Microsoft Caching Best Practices](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
- [Redis Best Practices](https://redis.io/docs/manual/patterns/)
- [Cache-Aside Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
