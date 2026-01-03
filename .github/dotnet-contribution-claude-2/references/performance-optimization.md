# 效能最佳化參考文件

## 快取架構設計

### 多層快取策略

```
┌───────────────────────────────────┐
│ L1 快取 (記憶體內快取)            │
│ IMemoryCache                      │
│ - 頻繁存取的小型資料              │
│ - 毫秒級存取速度                  │
│ - 單一實例限定                    │
└───────────────────────────────────┘
         ↓ (Miss)
┌───────────────────────────────────┐
│ L2 快取 (分散式快取)              │
│ Redis                             │
│ - 跨實例共用資料                  │
│ - 較大型資料                      │
│ - 支援叢集部署                    │
└───────────────────────────────────┘
         ↓ (Miss)
┌───────────────────────────────────┐
│ 資料來源                          │
│ SQL Server / 第三方 API           │
└───────────────────────────────────┘
```

### 快取鍵命名規範

**格式**：`{feature}:{operation}:{parameters}`

**範例**：
```csharp
// ✅ 良好的命名
"members:page:0:10"              // 會員列表分頁
"member:id:550e8400-e29b"        // 單一會員
"member:email:test@example.com"  // 依 Email 查詢
"products:category:electronics"  // 產品分類

// ❌ 不良的命名
"member"                         // 太模糊
"data"                           // 無意義
"cache_12345"                    // 無法理解
```

### 實作範例

```csharp
public class MemberCacheService(
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    IMemberRepository memberRepository,
    ILogger<MemberCacheService> logger)
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<Result<Member, Failure>> GetMemberAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        var cacheKey = $"member:id:{id}";

        // 1. 嘗試從 L1 快取（記憶體）取得
        if (memoryCache.TryGetValue(cacheKey, out Member? cachedMember))
        {
            logger.LogDebug("從 L1 快取取得會員: {MemberId}", id);
            return Result.Success<Member, Failure>(cachedMember!);
        }

        // 2. 嘗試從 L2 快取（Redis）取得
        var redisValue = await distributedCache.GetStringAsync(cacheKey, cancel);
        if (!string.IsNullOrEmpty(redisValue))
        {
            var member = JsonSerializer.Deserialize<Member>(redisValue);
            if (member != null)
            {
                // 回填 L1 快取
                memoryCache.Set(cacheKey, member, CacheExpiration);
                logger.LogDebug("從 L2 快取取得會員: {MemberId}", id);
                return Result.Success<Member, Failure>(member);
            }
        }

        // 3. 從資料庫查詢
        var result = await memberRepository.GetAsync(id, cancel);
        if (result.IsFailure)
            return result;

        // 4. 寫入快取
        var memberData = result.Value;

        // 寫入 L1 快取
        memoryCache.Set(cacheKey, memberData, CacheExpiration);

        // 寫入 L2 快取
        var json = JsonSerializer.Serialize(memberData);
        await distributedCache.SetStringAsync(
            cacheKey,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration
            },
            cancel);

        logger.LogDebug("從資料庫取得會員並寫入快取: {MemberId}", id);
        return result;
    }

    public async Task InvalidateMemberCacheAsync(Guid id)
    {
        var cacheKey = $"member:id:{id}";

        // 清除 L1 快取
        memoryCache.Remove(cacheKey);

        // 清除 L2 快取
        await distributedCache.RemoveAsync(cacheKey);

        logger.LogDebug("清除會員快取: {MemberId}", id);
    }
}
```

📝 **快取實作參考**: [src/be/JobBank1111.Infrastructure/Caching/](../../src/be/JobBank1111.Infrastructure/Caching/)

### 快取備援策略

```csharp
public class CacheService(
    IDistributedCache distributedCache,
    IMemoryCache memoryCache,
    ILogger<CacheService> logger)
{
    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancel = default)
    {
        try
        {
            // 嘗試從 Redis 取得
            var redisValue = await distributedCache.GetStringAsync(key, cancel);
            if (!string.IsNullOrEmpty(redisValue))
            {
                return JsonSerializer.Deserialize<T>(redisValue);
            }
        }
        catch (Exception ex)
        {
            // ✅ Redis 不可用時，降級至記憶體快取
            logger.LogWarning(ex, "Redis 不可用，降級至記憶體快取");

            if (memoryCache.TryGetValue(key, out T? cachedValue))
            {
                return cachedValue;
            }
        }

        // 執行工廠方法取得資料
        var value = await factory();

        // 寫入快取
        try
        {
            var json = JsonSerializer.Serialize(value);
            await distributedCache.SetStringAsync(
                key,
                json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration },
                cancel);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "無法寫入 Redis，寫入記憶體快取");
            memoryCache.Set(key, value, expiration);
        }

        return value;
    }
}
```

## 快取失效與管理

### 時間過期（TTL）

```csharp
// ✅ 根據資料特性設定適當的 TTL
public static class CacheTTL
{
    public static readonly TimeSpan Static = TimeSpan.FromHours(24);      // 靜態資料（如分類）
    public static readonly TimeSpan SemiStatic = TimeSpan.FromHours(1);   // 半靜態資料（如產品資訊）
    public static readonly TimeSpan Dynamic = TimeSpan.FromMinutes(10);   // 動態資料（如庫存）
    public static readonly TimeSpan RealTime = TimeSpan.FromMinutes(1);   // 即時資料（如購物車）
}

// 使用範例
memoryCache.Set(
    "categories:all",
    categories,
    CacheTTL.Static  // 靜態資料可以快取較久
);

memoryCache.Set(
    $"product:stock:{productId}",
    stock,
    CacheTTL.Dynamic  // 庫存資訊需要較短的 TTL
);
```

### 事件驅動快取失效

```csharp
public class MemberService(
    IMemberRepository memberRepository,
    IMemberCacheService cacheService)
{
    public async Task<Result<Member, Failure>> UpdateMemberAsync(
        Member member,
        CancellationToken cancel = default)
    {
        // 1. 更新資料庫
        var result = await memberRepository.UpdateAsync(member, cancel);

        if (result.IsSuccess)
        {
            // 2. ✅ 清除相關快取
            await cacheService.InvalidateMemberCacheAsync(member.Id);

            // 3. 如果有相關的列表快取，也需要清除
            await cacheService.InvalidateMemberListCacheAsync();
        }

        return result;
    }
}
```

## ASP.NET Core 效能最佳化

### 連線池（DbContext Pool）

```csharp
// Program.cs
// ✅ 使用 AddDbContextPool 重用 DbContext 實例
builder.Services.AddDbContextPool<AppDbContext>(
    options => options.UseSqlServer(connectionString),
    poolSize: 128  // 預設 128，可根據負載調整
);

// ⚠️ 注意：使用 DbContextFactory 時不需要設定 pooling
// DbContextFactory 內部已經處理
builder.Services.AddDbContextFactory<AppDbContext>(
    options => options.UseSqlServer(connectionString)
);
```

### 回應壓縮

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;  // HTTPS 也啟用壓縮
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;  // 平衡壓縮率與速度
});

// 使用中介軟體
app.UseResponseCompression();
```

### 輸出快取（ASP.NET Core 8.0+）

```csharp
// Program.cs
builder.Services.AddOutputCache(options =>
{
    // 預設快取策略
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .Tag("default"));

    // 自訂策略
    options.AddPolicy("StaticContent", builder => builder
        .Expire(TimeSpan.FromHours(24))
        .Tag("static"));
});

app.UseOutputCache();

// Controller 使用
[HttpGet]
[OutputCache(PolicyName = "StaticContent")]
public IActionResult GetCategories()
{
    // 回應會被快取 24 小時
}
```

## 資料庫查詢最佳化

### 使用 AsNoTracking()

```csharp
// ✅ 唯讀查詢使用 AsNoTracking()
var members = await dbContext.Members
    .AsNoTracking()  // ✅ 提升效能 20-30%
    .Where(m => m.IsActive)
    .ToListAsync(cancel);

// ⚠️ 需要更新的查詢不使用 AsNoTracking()
var member = await dbContext.Members.FindAsync(id);
member.Name = "Updated";
await dbContext.SaveChangesAsync();  // ✅ 會追蹤變更
```

### 投影查詢（Select）

```csharp
// ❌ 不佳：載入完整實體
var members = await dbContext.Members
    .Include(m => m.Orders)  // ❌ 載入所有 Orders
    .ToListAsync();

// ✅ 更好：只載入需要的欄位
var memberSummaries = await dbContext.Members
    .Select(m => new MemberSummary
    {
        Id = m.Id,
        Name = m.Name,
        Email = m.Email,
        OrderCount = m.Orders.Count  // ✅ 只計算數量
    })
    .AsNoTracking()
    .ToListAsync(cancel);
```

### 批次操作

```csharp
// ✅ 批次插入（使用 AddRange）
public async Task<Result> ImportMembersAsync(
    List<Member> members,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // ✅ 批次操作比逐一操作快很多
    dbContext.Members.AddRange(members);

    await dbContext.SaveChangesAsync(cancel);

    return Result.Success();
}

// 進階：使用 EFCore.BulkExtensions（第三方套件）
public async Task<Result> BulkImportMembersAsync(
    List<Member> members,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // ✅ 大量資料時使用 BulkInsert（效能提升 10-100 倍）
    await dbContext.BulkInsertAsync(members, cancel);

    return Result.Success();
}
```

## 記憶體管理

### 使用 ArrayPool<T>

```csharp
// ❌ 不佳：每次建立新陣列
public byte[] ProcessData(int size)
{
    var buffer = new byte[size];  // ❌ 產生垃圾收集壓力
    // ... 處理資料
    return buffer;
}

// ✅ 更好：使用 ArrayPool 重用陣列
public byte[] ProcessData(int size)
{
    var buffer = ArrayPool<byte>.Shared.Rent(size);  // ✅ 從池中租用
    try
    {
        // ... 處理資料
        var result = buffer[..size];  // 複製需要的部分
        return result;
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);  // ✅ 歸還到池中
    }
}
```

### 使用 Span<T> 與 Memory<T>

```csharp
// ❌ 不佳：字串操作產生大量暫存物件
public string FormatName(string firstName, string lastName)
{
    return $"{firstName} {lastName}".Trim();  // ❌ 多次字串配置
}

// ✅ 更好：使用 Span<T> 減少配置
public string FormatName(ReadOnlySpan<char> firstName, ReadOnlySpan<char> lastName)
{
    Span<char> buffer = stackalloc char[firstName.Length + 1 + lastName.Length];
    firstName.CopyTo(buffer);
    buffer[firstName.Length] = ' ';
    lastName.CopyTo(buffer[(firstName.Length + 1)..]);
    return new string(buffer.Trim());
}
```

### StringBuilder 最佳化

```csharp
// ❌ 不佳：字串串接
public string BuildSql(List<string> columns)
{
    string sql = "SELECT ";
    foreach (var column in columns)
    {
        sql += column + ", ";  // ❌ 每次都建立新字串
    }
    return sql.TrimEnd(',', ' ');
}

// ✅ 更好：使用 StringBuilder
public string BuildSql(List<string> columns)
{
    var sb = new StringBuilder("SELECT ");
    foreach (var column in columns)
    {
        sb.Append(column).Append(", ");
    }
    sb.Length -= 2;  // 移除最後的 ", "
    return sb.ToString();
}

// ✅ 最佳：預先分配容量
public string BuildSql(List<string> columns)
{
    var estimatedLength = 7 + columns.Sum(c => c.Length + 2);  // 預估長度
    var sb = new StringBuilder(estimatedLength);
    sb.Append("SELECT ");
    foreach (var column in columns)
    {
        sb.Append(column).Append(", ");
    }
    sb.Length -= 2;
    return sb.ToString();
}
```

## 非同步程式設計最佳化

### 避免不必要的 Task.Run

```csharp
// ❌ 錯誤：不需要的 Task.Run
public async Task<Member> GetMemberAsync(Guid id)
{
    return await Task.Run(async () =>  // ❌ 不必要的執行緒切換
    {
        return await memberRepository.GetAsync(id);
    });
}

// ✅ 正確：直接 await
public async Task<Member> GetMemberAsync(Guid id, CancellationToken cancel)
{
    return await memberRepository.GetAsync(id, cancel);  // ✅ 直接 await
}
```

### ConfigureAwait(false)

```csharp
// ⚠️ 在 ASP.NET Core 中不需要 ConfigureAwait(false)
// ASP.NET Core 沒有 SynchronizationContext，不會造成死鎖

// ✅ Library 程式碼可以使用
public async Task<Member> GetMemberAsync(Guid id)
{
    var member = await dbContext.Members
        .FindAsync(id)
        .ConfigureAwait(false);  // ✅ Library 中使用

    return member;
}
```

## 效能監控與分析

### 使用 BenchmarkDotNet

```csharp
[MemoryDiagnoser]
public class MemberRepositoryBenchmark
{
    private IDbContextFactory<AppDbContext> _factory = null!;

    [GlobalSetup]
    public void Setup()
    {
        // 設定測試環境
    }

    [Benchmark(Baseline = true)]
    public async Task<List<Member>> GetMembers_WithTracking()
    {
        await using var dbContext = await _factory.CreateDbContextAsync();
        return await dbContext.Members.ToListAsync();
    }

    [Benchmark]
    public async Task<List<Member>> GetMembers_NoTracking()
    {
        await using var dbContext = await _factory.CreateDbContextAsync();
        return await dbContext.Members.AsNoTracking().ToListAsync();
    }
}
```

### 效能計數器

```csharp
public class MemberService(
    IMemberRepository memberRepository,
    ILogger<MemberService> logger)
{
    private static readonly Counter<long> MemberCreatedCounter =
        Meter.CreateCounter<long>("members.created");

    public async Task<Result<Member>> CreateMemberAsync(CreateMemberRequest request)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await memberRepository.CreateAsync(request);

        stopwatch.Stop();

        // 記錄度量
        MemberCreatedCounter.Add(1);
        logger.LogInformation(
            "建立會員耗時 {ElapsedMs} ms",
            stopwatch.ElapsedMilliseconds);

        return result;
    }
}
```

## 效能最佳化檢查清單

### 資料庫查詢
- [ ] 使用 AsNoTracking() 於唯讀查詢
- [ ] 避免 N+1 查詢（使用 Include/Select）
- [ ] 使用分頁查詢（Skip/Take）
- [ ] 使用投影查詢（Select）只載入需要的欄位
- [ ] 批次操作使用 AddRange/RemoveRange

### 快取策略
- [ ] 實作多層快取（L1 記憶體 + L2 Redis）
- [ ] 設定適當的 TTL
- [ ] 實作快取失效機制
- [ ] 使用快取鍵命名規範

### 記憶體管理
- [ ] 使用 ArrayPool 重用陣列
- [ ] 使用 Span<T> / Memory<T> 減少配置
- [ ] 使用 StringBuilder 處理字串串接

### 非同步程式設計
- [ ] 所有 I/O 操作使用 async/await
- [ ] 避免不必要的 Task.Run
- [ ] 傳遞 CancellationToken

### ASP.NET Core
- [ ] 啟用回應壓縮
- [ ] 使用輸出快取
- [ ] 使用 DbContextFactory

## 參考資源

- 📚 [CLAUDE.md](../../../CLAUDE.md) - 完整專案指導文件
- 📝 [EF Core 最佳實踐](./ef-core-best-practices.md) - 資料庫查詢最佳化
- 📝 [快取實作](../../src/be/JobBank1111.Infrastructure/Caching/) - 快取服務範例
