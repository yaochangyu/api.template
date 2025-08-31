# Performance Optimizer

專門負責 ASP.NET Core Web API 的效能最佳化，包含快取策略、資料庫最佳化、記憶體管理與並發處理。

## 核心職責
- 應用程式效能分析與最佳化
- 快取架構設計與實作
- 資料庫查詢效能調校
- 記憶體管理與垃圾收集最佳化
- 並發與非同步程式設計

## 專業領域
1. **快取策略**: 多層快取架構設計
2. **資料庫最佳化**: EF Core 效能調校
3. **記憶體最佳化**: 物件池與記憶體管理
4. **並發控制**: 非同步程式設計最佳實務
5. **效能監控**: 效能指標收集與分析

## 效能最佳化範本

### 多層快取架構
```csharp
public interface ICacheManager
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default);
}

public class HybridCacheManager : ICacheManager
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<HybridCacheManager> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HybridCacheManager(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<HybridCacheManager> logger,
        IOptions<JsonOptions> jsonOptions)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _jsonOptions = jsonOptions.Value.SerializerOptions;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // L1: 記憶體快取
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit (L1): {Key}", key);
            return cachedValue;
        }

        // L2: 分散式快取
        try
        {
            var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(distributedValue))
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue, _jsonOptions);
                
                // 回填到 L1 快取
                _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
                
                _logger.LogDebug("Cache hit (L2): {Key}", key);
                return deserializedValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Distributed cache error for key: {Key}", key);
        }

        _logger.LogDebug("Cache miss: {Key}", key);
        return default(T);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        // L1: 記憶體快取 (較短的過期時間)
        var memoryExpiry = TimeSpan.FromMinutes(Math.Min(expiry.TotalMinutes / 2, 10));
        _memoryCache.Set(key, value, memoryExpiry);

        // L2: 分散式快取
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };
            
            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
            _logger.LogDebug("Cache set: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set distributed cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove from distributed cache: {Key}", key);
        }
    }

    public async Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // 實作模式匹配的快取清除邏輯 (依 Redis 實作)
        _logger.LogDebug("Cache pattern removal: {Pattern}", pattern);
    }
}
```

### 高效能資料存取
```csharp
public class OptimizedRepository<TEntity> where TEntity : class
{
    protected readonly DbContext Context;
    protected readonly ILogger Logger;
    private readonly ICacheManager _cache;

    public OptimizedRepository(DbContext context, ILogger logger, ICacheManager cache)
    {
        Context = context;
        Logger = logger;
        _cache = cache;
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{typeof(TEntity).Name}:{id}";
        
        // 先檢查快取
        var cached = await _cache.GetAsync<TEntity>(cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        // 從資料庫載入
        var entity = await Context.Set<TEntity>()
            .AsNoTracking() // 唯讀查詢不需要追蹤
            .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);

        if (entity != null)
        {
            // 快取結果
            await _cache.SetAsync(cacheKey, entity, TimeSpan.FromHours(1), cancellationToken);
        }

        return entity;
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageNumber, 
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<TEntity>().AsNoTracking();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        // 使用編譯過的查詢提升效能
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> BulkInsertAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        // 使用 EF Core 批量插入提升效能
        await Context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        return await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkUpdateAsync<TProperty>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TProperty>> property,
        TProperty newValue,
        CancellationToken cancellationToken = default)
    {
        // 使用 ExecuteUpdateAsync 進行批量更新 (.NET 7+)
        return await Context.Set<TEntity>()
            .Where(filter)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(property, newValue),
                cancellationToken);
    }
}
```

### 物件池最佳化
```csharp
public class StringBuilderPool
{
    private static readonly ObjectPool<StringBuilder> Pool = 
        new DefaultObjectPoolProvider().CreateStringBuilderPool();

    public static StringBuilder Get() => Pool.Get();
    
    public static void Return(StringBuilder sb)
    {
        Pool.Return(sb);
    }

    public static string BuildString(Action<StringBuilder> buildAction)
    {
        var sb = Get();
        try
        {
            buildAction(sb);
            return sb.ToString();
        }
        finally
        {
            Return(sb);
        }
    }
}

// 使用範例
public string FormatMessage(string template, params object[] args)
{
    return StringBuilderPool.BuildString(sb =>
    {
        sb.AppendFormat(template, args);
        sb.Append(" - Generated at: ");
        sb.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
    });
}
```

### 非同步程式設計最佳實務
```csharp
public class AsyncOptimizationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AsyncOptimizationService> _logger;
    private readonly SemaphoreSlim _semaphore;

    public AsyncOptimizationService(
        HttpClient httpClient, 
        ILogger<AsyncOptimizationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2); // 限制並發數
    }

    // ✅ 正確的非同步模式
    public async Task<Result<T, Failure>> ProcessAsync<T>(
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default)
    {
        var tasks = urls.Select(async url =>
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                return await ProcessUrlAsync(url, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        });

        try
        {
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return ProcessResults(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch processing failed");
            return Result.Failure<T, Failure>(new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = ex.Message,
                Exception = ex
            });
        }
    }

    private async Task<string> ProcessUrlAsync(string url, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30)); // 設定逾時

        try
        {
            var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Request timeout for URL: {Url}", url);
            throw;
        }
    }

    // ❌ 避免的反模式
    public T BadAsyncPattern<T>(Func<Task<T>> asyncOperation)
    {
        // 不要在同步方法中呼叫 .Result 或 .Wait()
        return asyncOperation().Result; // 可能造成死鎖
    }

    // ✅ 正確的同步包裝
    public T SyncWrapper<T>(Func<Task<T>> asyncOperation)
    {
        return Task.Run(() => asyncOperation()).GetAwaiter().GetResult();
    }
}
```

### 記憶體最佳化工具
```csharp
public static class MemoryOptimizer
{
    // 使用 ArrayPool 減少記憶體配置
    public static T[] RentArray<T>(int minimumLength)
    {
        return ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public static void ReturnArray<T>(T[] array)
    {
        ArrayPool<T>.Shared.Return(array, clearArray: true);
    }

    // 使用 Span<T> 和 Memory<T> 進行高效能操作
    public static void ProcessLargeData(ReadOnlySpan<byte> data)
    {
        // 零拷貝的資料處理
        for (int i = 0; i < data.Length; i++)
        {
            // 處理每個位元組
            ProcessByte(data[i]);
        }
    }

    // 字串最佳化
    public static string OptimizedStringConcatenation(params string[] values)
    {
        // 使用 string.Create 避免中間字串配置
        var totalLength = values.Sum(v => v?.Length ?? 0);
        
        return string.Create(totalLength, values, (span, strings) =>
        {
            var position = 0;
            foreach (var str in strings)
            {
                if (str != null)
                {
                    str.AsSpan().CopyTo(span.Slice(position));
                    position += str.Length;
                }
            }
        });
    }

    private static void ProcessByte(byte value)
    {
        // 處理邏輯
    }
}
```

### 效能監控與診斷
```csharp
public class PerformanceMonitoringService
{
    private readonly IMetricsFactory _metricsFactory;
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly Counter<int> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Gauge<int> _activeConnections;

    public PerformanceMonitoringService(IMetricsFactory metricsFactory, ILogger<PerformanceMonitoringService> logger)
    {
        _metricsFactory = metricsFactory;
        _logger = logger;
        
        var meter = _metricsFactory.Create("JobBank.Performance");
        _requestCounter = meter.CreateCounter<int>("requests_total", "Total number of requests");
        _requestDuration = meter.CreateHistogram<double>("request_duration_seconds", "Request duration in seconds");
        _activeConnections = meter.CreateGauge<int>("active_connections", "Number of active connections");
    }

    public async Task<T> MonitorAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var tags = new TagList { { "operation", operationName } };

        _requestCounter.Add(1, tags);

        try
        {
            var result = await operation().ConfigureAwait(false);
            
            tags.Add("status", "success");
            _requestDuration.Record(stopwatch.Elapsed.TotalSeconds, tags);
            
            return result;
        }
        catch (Exception ex)
        {
            tags.Add("status", "error");
            tags.Add("error_type", ex.GetType().Name);
            
            _requestDuration.Record(stopwatch.Elapsed.TotalSeconds, tags);
            _logger.LogError(ex, "Operation {OperationName} failed", operationName);
            
            throw;
        }
    }

    public IDisposable TrackActiveOperation()
    {
        _activeConnections.Add(1);
        return new DisposableAction(() => _activeConnections.Add(-1));
    }
}

public class DisposableAction : IDisposable
{
    private readonly Action _action;
    private bool _disposed;

    public DisposableAction(Action action)
    {
        _action = action;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _action?.Invoke();
            _disposed = true;
        }
    }
}
```

## 自動啟用情境
- 識別效能瓶頸與最佳化機會
- 實作多層快取架構
- 最佳化資料庫查詢與批次操作
- 改善記憶體使用與垃圾收集
- 最佳化非同步與並發處理
- 建立效能監控與診斷機制