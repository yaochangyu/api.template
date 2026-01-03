# 快取策略

> 本專案採用多層快取策略，結合 Redis 與記憶體快取提供高效能資料存取

## 快取架構設計

### 多層快取策略

```
                    查詢請求
                       ↓
           ┌───────────────────────┐
           │   L1: Memory Cache    │ ← 最快，僅本機可用
           │   (IMemoryCache)      │
           └───────────┬───────────┘
                       ↓ Miss
           ┌───────────────────────┐
           │   L2: Redis Cache     │ ← 快速，跨實例共用
           │   (Distributed)       │
           └───────────┬───────────┘
                       ↓ Miss
           ┌───────────────────────┐
           │   Database            │ ← 最慢，持久化儲存
           └───────────────────────┘
```

### L1 快取：記憶體內快取 (IMemoryCache)

**特性**：
- 最快的存取速度
- 僅限單一應用程式實例
- 自動記憶體管理

**適用場景**：
- 頻繁存取的小型資料
- 不常變動的參考資料
- 單實例應用程式

**範例**：
```csharp
public class CacheService(IMemoryCache memoryCache)
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration)
    {
        // 嘗試從記憶體快取取得
        if (memoryCache.TryGetValue(key, out T value))
        {
            return value;
        }

        // 快取 Miss，執行工廠方法
        value = await factory();

        // 寫入快取
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        memoryCache.Set(key, value, cacheOptions);

        return value;
    }
}
```

### L2 快取：Redis 分散式快取

**特性**：
- 跨應用程式實例共用
- 持久化選項
- 支援複雜資料結構

**適用場景**：
- 多實例部署
- 需要跨實例共用的資料
- Session 管理
- 分散式鎖

**範例**：
```csharp
public class RedisCache Service(IDistributedCache distributedCache)
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancel = default)
    {
        // 嘗試從 Redis 取得
        var cachedValue = await distributedCache.GetStringAsync(key, cancel);
        
        if (!string.IsNullOrEmpty(cachedValue))
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        // 快取 Miss，執行工廠方法
        var value = await factory();

        // 寫入 Redis
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        
        var serializedValue = JsonSerializer.Serialize(value);
        await distributedCache.SetStringAsync(key, serializedValue, options, cancel);

        return value;
    }
}
```

### 快取備援策略

**核心原則**：當 Redis 不可用時，自動降級至記憶體快取

```csharp
public class HybridCacheService(
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    ILogger<HybridCacheService> logger)
{
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancel = default)
    {
        try
        {
            // 優先使用 Redis
            return await GetFromRedisAsync<T>(key, factory, expiration, cancel);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis unavailable, falling back to memory cache");
            
            // Redis 失敗，降級到記憶體快取
            return await GetFromMemoryAsync<T>(key, factory, expiration);
        }
    }
}
```

## 快取鍵命名規範

### 命名格式

```
{feature}:{operation}:{parameters}
```

### 範例

```csharp
// 會員分頁查詢
"members:page:0:10"

// 單一會員查詢（by ID）
"member:id:550e8400-e29b-41d4-a716-446655440000"

// 單一會員查詢（by Email）
"member:email:test@example.com"

// 產品分類列表
"products:categories:all"

// 特定分類的產品
"products:category:electronics"
```

### 最佳實踐

```csharp
public static class CacheKeys
{
    // 使用靜態方法產生快取鍵
    public static string MemberById(Guid id) => $"member:id:{id}";
    
    public static string MemberByEmail(string email) => $"member:email:{email}";
    
    public static string MembersPage(int pageNumber, int pageSize) =>
        $"members:page:{pageNumber}:{pageSize}";
    
    // 使用命名空間區分不同功能
    public const string MemberPrefix = "member:";
    public const string ProductPrefix = "product:";
    public const string OrderPrefix = "order:";
}

// 使用方式
var cacheKey = CacheKeys.MemberById(memberId);
```

## 快取過期策略

### 時間過期 (TTL - Time To Live)

```csharp
public class CacheExpirations
{
    // 靜態參考資料（長時間）
    public static readonly TimeSpan StaticData = TimeSpan.FromHours(24);
    
    // 一般業務資料（中等時間）
    public static readonly TimeSpan BusinessData = TimeSpan.FromMinutes(30);
    
    // 頻繁變動資料（短時間）
    public static readonly TimeSpan VolatileData = TimeSpan.FromMinutes(5);
    
    // Session 資料
    public static readonly TimeSpan Session = TimeSpan.FromHours(2);
}

// 使用方式
await cacheService.SetAsync(
    key, 
    value, 
    CacheExpirations.BusinessData);
```

### 滑動過期 (Sliding Expiration)

```csharp
var options = new MemoryCacheEntryOptions
{
    // 20 分鐘內未存取則過期
    SlidingExpiration = TimeSpan.FromMinutes(20)
};
```

### 絕對過期 (Absolute Expiration)

```csharp
var options = new MemoryCacheEntryOptions
{
    // 在指定時間點過期
    AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)
};
```

## 快取失效管理

### 主動清除快取

```csharp
public class MemberRepository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IDistributedCache cache)
{
    public async Task<Result<Member>> UpdateAsync(
        Member member,
        CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        
        dbContext.Members.Update(member);
        await dbContext.SaveChangesAsync(cancel);
        
        // 主動清除相關快取
        await cache.RemoveAsync(CacheKeys.MemberById(member.Id), cancel);
        await cache.RemoveAsync(CacheKeys.MemberByEmail(member.Email), cancel);
        
        return Result.Success<Member, Failure>(member);
    }
}
```

### 版本控制策略

```csharp
public static class CacheKeys
{
    private const string Version = "v1";  // 快取版本
    
    public static string MemberById(Guid id) => $"{Version}:member:id:{id}";
    
    // 需要全面清除快取時，只需更新版本號
    // 舊版本的快取會自然過期
}
```

### 標籤快取（批次清除）

```csharp
public class TaggedCacheService
{
    // 使用標籤管理相關快取
    public async Task SetWithTagAsync<T>(
        string key,
        T value,
        string[] tags,
        TimeSpan expiration)
    {
        // 儲存資料
        await cache.SetAsync(key, value, expiration);
        
        // 為每個標籤記錄此快取鍵
        foreach (var tag in tags)
        {
            await AddKeyToTagAsync(tag, key);
        }
    }
    
    // 根據標籤清除所有相關快取
    public async Task InvalidateTagAsync(string tag)
    {
        var keys = await GetKeysForTagAsync(tag);
        
        foreach (var key in keys)
        {
            await cache.RemoveAsync(key);
        }
        
        await RemoveTagAsync(tag);
    }
}

// 使用範例
await taggedCache.SetWithTagAsync(
    "member:id:123",
    member,
    new[] { "member", "user-data" },
    TimeSpan.FromMinutes(30));

// 清除所有會員相關快取
await taggedCache.InvalidateTagAsync("member");
```

## 快取穿透、擊穿、雪崩防護

### 快取穿透（Cache Penetration）

**問題**：查詢不存在的資料，導致每次都查詢資料庫

**解決方案**：快取空值
```csharp
public async Task<Member> GetByIdAsync(Guid id, CancellationToken cancel = default)
{
    var cacheKey = CacheKeys.MemberById(id);
    
    // 嘗試從快取取得
    var cached = await cache.GetAsync<Member>(cacheKey, cancel);
    if (cached != null)
    {
        return cached;
    }
    
    // 從資料庫查詢
    var member = await dbContext.Members.FindAsync(id, cancel);
    
    // 即使是 null 也快取起來（使用較短的過期時間）
    var expiration = member != null 
        ? CacheExpirations.BusinessData 
        : TimeSpan.FromMinutes(5);  // 空值快取時間較短
    
    await cache.SetAsync(cacheKey, member, expiration, cancel);
    
    return member;
}
```

### 快取擊穿（Cache Breakdown）

**問題**：熱點資料過期，大量請求同時查詢資料庫

**解決方案**：使用分散式鎖
```csharp
public async Task<Member> GetByIdAsync(Guid id, CancellationToken cancel = default)
{
    var cacheKey = CacheKeys.MemberById(id);
    var lockKey = $"lock:{cacheKey}";
    
    // 嘗試從快取取得
    var cached = await cache.GetAsync<Member>(cacheKey, cancel);
    if (cached != null)
    {
        return cached;
    }
    
    // 取得分散式鎖
    var acquired = await distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(10));
    
    if (acquired)
    {
        try
        {
            // 再次檢查快取（Double-Check）
            cached = await cache.GetAsync<Member>(cacheKey, cancel);
            if (cached != null)
            {
                return cached;
            }
            
            // 從資料庫查詢
            var member = await dbContext.Members.FindAsync(id, cancel);
            
            // 寫入快取
            await cache.SetAsync(cacheKey, member, CacheExpirations.BusinessData, cancel);
            
            return member;
        }
        finally
        {
            await distributedLock.ReleaseAsync(lockKey);
        }
    }
    else
    {
        // 未取得鎖，等待後重試
        await Task.Delay(100, cancel);
        return await GetByIdAsync(id, cancel);
    }
}
```

### 快取雪崩（Cache Avalanche）

**問題**：大量快取同時過期，導致資料庫壓力暴增

**解決方案**：過期時間加上隨機值
```csharp
public async Task SetAsync<T>(
    string key,
    T value,
    TimeSpan baseExpiration,
    CancellationToken cancel = default)
{
    // 在基礎過期時間上加上隨機值（0-60 秒）
    var random = new Random();
    var jitter = TimeSpan.FromSeconds(random.Next(0, 60));
    var expiration = baseExpiration.Add(jitter);
    
    await cache.SetAsync(key, value, expiration, cancel);
}
```

## 快取預熱

**概念**：應用程式啟動時預先載入常用資料

```csharp
public class CacheWarmupService(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IDistributedCache cache) : IHostedService
{
    public async Task StartAsync(CancellationToken cancel)
    {
        // 預熱：載入產品分類
        await WarmupCategoriesAsync(cancel);
        
        // 預熱：載入常用配置
        await WarmupConfigurationsAsync(cancel);
    }
    
    private async Task WarmupCategoriesAsync(CancellationToken cancel)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        
        var categories = await dbContext.Categories.ToListAsync(cancel);
        
        await cache.SetAsync(
            "products:categories:all",
            categories,
            CacheExpirations.StaticData,
            cancel);
    }
    
    public Task StopAsync(CancellationToken cancel) => Task.CompletedTask;
}

// 註冊服務
builder.Services.AddHostedService<CacheWarmupService>();
```

## 最佳實踐檢查清單

### 快取鍵設計
- [ ] 使用結構化的命名格式
- [ ] 定義 CacheKeys 靜態類別
- [ ] 包含版本號（便於全面清除）

### 過期策略
- [ ] 根據資料特性設定合理的 TTL
- [ ] 考慮使用滑動過期（頻繁存取的資料）
- [ ] 過期時間加上隨機值（防止雪崩）

### 快取更新
- [ ] 資料異動時主動清除快取
- [ ] 考慮使用標籤管理相關快取
- [ ] 避免快取與資料庫不一致

### 效能與安全
- [ ] 使用分散式鎖防止快取擊穿
- [ ] 快取空值防止快取穿透
- [ ] 實作快取備援（Redis → Memory）

---

**參考來源**：CLAUDE.md - 效能最佳化與快取策略章節  
**實作位置**：`src/be/JobBank1111.Infrastructure/Caching/`
