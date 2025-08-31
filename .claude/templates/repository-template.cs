using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.Caching;
using JobBank1111.Infrastructure.TraceContext;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.{{ENTITY}};

public class {{ENTITY}}Repository(
    JobBankDbContext dbContext,
    ICacheProvider cache,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<{{ENTITY}}Repository> logger)
{
    private const string CacheKeyPrefix = "{{ENTITY}}_";
    private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<Result<{{ENTITY}}, Failure>> CreateAsync(
        Create{{ENTITY}}Request request, 
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        try
        {
            var entity = new {{ENTITY}}
            {
                Name = request.Name,
                // TODO: 對應其他屬性
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.{{ENTITY}}s.Add(entity);
            await dbContext.SaveChangesAsync(cancel);

            // 清除相關快取
            await InvalidateRelatedCacheAsync(entity.Id);

            logger.LogInformation("成功建立 {{ENTITY}}: {EntityId} - TraceId: {TraceId}", 
                entity.Id, traceContext?.TraceId);

            return Result.Success<{{ENTITY}}, Failure>(entity);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "建立 {{ENTITY}} 時發生資料庫錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<{{ENTITY}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "建立資料時發生錯誤",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "建立 {{ENTITY}} 時發生未預期錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<{{ENTITY}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.Unknown),
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    public async Task<Result<{{ENTITY}}, Failure>> UpdateAsync(
        int id, 
        Update{{ENTITY}}Request request, 
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        try
        {
            var entity = await dbContext.{{ENTITY}}s
                .FirstOrDefaultAsync(x => x.Id == id, cancel);

            if (entity == null)
            {
                return Result.Failure<{{ENTITY}}, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "找不到指定的 {{ENTITY}}",
                    TraceId = traceContext?.TraceId
                });
            }

            // 更新欄位
            entity.Name = request.Name;
            // TODO: 更新其他屬性
            entity.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancel);

            // 清除相關快取
            await InvalidateRelatedCacheAsync(id);

            logger.LogInformation("成功更新 {{ENTITY}}: {EntityId} - TraceId: {TraceId}", 
                id, traceContext?.TraceId);

            return Result.Success<{{ENTITY}}, Failure>(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "更新 {{ENTITY}} 時發生併發衝突 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<{{ENTITY}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbConcurrency),
                Message = "資料已被其他使用者修改",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "更新 {{ENTITY}} 時發生資料庫錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<{{ENTITY}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "更新資料時發生錯誤",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    public async Task<Result<{{ENTITY}}, Failure>> GetByIdAsync(
        int id, 
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        var cacheKey = $"{CacheKeyPrefix}{id}";
        
        try
        {
            // 先從快取取得
            var cachedEntity = await cache.GetAsync<{{ENTITY}}>(cacheKey, cancel);
            if (cachedEntity != null)
            {
                logger.LogDebug("從快取取得 {{ENTITY}}: {EntityId} - TraceId: {TraceId}", 
                    id, traceContext?.TraceId);
                return Result.Success<{{ENTITY}}, Failure>(cachedEntity);
            }

            // 從資料庫查詢
            var entity = await dbContext.{{ENTITY}}s
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancel);

            if (entity == null)
            {
                return Result.Failure<{{ENTITY}}, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "找不到指定的 {{ENTITY}}",
                    TraceId = traceContext?.TraceId
                });
            }

            // 存入快取
            await cache.SetAsync(cacheKey, entity, CacheDuration, cancel);

            logger.LogDebug("從資料庫取得 {{ENTITY}}: {EntityId} - TraceId: {TraceId}", 
                id, traceContext?.TraceId);

            return Result.Success<{{ENTITY}}, Failure>(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "查詢 {{ENTITY}} 時發生錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<{{ENTITY}}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "查詢資料時發生錯誤",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    public async Task<Result<PaginatedList<Get{{ENTITY}}Response>, Failure>> GetOffsetAsync(
        int pageIndex, 
        int pageSize, 
        bool noCache = true, 
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        var cacheKey = $"{CacheKeyPrefix}offset_{pageIndex}_{pageSize}";
        
        try
        {
            // 如果不禁用快取，先嘗試從快取取得
            if (!noCache)
            {
                var cachedResult = await cache.GetAsync<PaginatedList<Get{{ENTITY}}Response>>(cacheKey, cancel);
                if (cachedResult != null)
                {
                    logger.LogDebug("從快取取得分頁資料 - TraceId: {TraceId}", traceContext?.TraceId);
                    return Result.Success<PaginatedList<Get{{ENTITY}}Response>, Failure>(cachedResult);
                }
            }

            // 計算總數
            var totalCount = await dbContext.{{ENTITY}}s.CountAsync(cancel);

            // 查詢分頁資料
            var entities = await dbContext.{{ENTITY}}s
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => new Get{{ENTITY}}Response
                {
                    Id = x.Id,
                    Name = x.Name,
                    // TODO: 對應其他屬性
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancel);

            var result = new PaginatedList<Get{{ENTITY}}Response>
            {
                Items = entities,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = pageIndex > 0,
                HasNextPage = (pageIndex + 1) * pageSize < totalCount
            };

            // 存入快取
            if (!noCache)
            {
                await cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancel);
            }

            logger.LogInformation("查詢分頁資料成功，頁數: {PageIndex}, 大小: {PageSize}, 總數: {TotalCount} - TraceId: {TraceId}", 
                pageIndex, pageSize, totalCount, traceContext?.TraceId);

            return Result.Success<PaginatedList<Get{{ENTITY}}Response>, Failure>(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "查詢分頁資料時發生錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<PaginatedList<Get{{ENTITY}}Response>, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "查詢分頁資料時發生錯誤",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    public async Task<Result<CursorPaginatedList<Get{{ENTITY}}Response>, Failure>> GetCursorAsync(
        int pageSize, 
        string nextPageToken, 
        bool noCache = true, 
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        try
        {
            var query = dbContext.{{ENTITY}}s.AsNoTracking();

            // 解析游標
            int startId = 0;
            if (!string.IsNullOrEmpty(nextPageToken) && int.TryParse(nextPageToken, out var parsedId))
            {
                startId = parsedId;
                query = query.Where(x => x.Id > startId);
            }

            var entities = await query
                .OrderBy(x => x.Id)
                .Take(pageSize + 1) // 多取一筆判斷是否還有下一頁
                .Select(x => new Get{{ENTITY}}Response
                {
                    Id = x.Id,
                    Name = x.Name,
                    // TODO: 對應其他屬性
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancel);

            var hasNextPage = entities.Count > pageSize;
            if (hasNextPage)
            {
                entities = entities.Take(pageSize).ToList();
            }

            var result = new CursorPaginatedList<Get{{ENTITY}}Response>
            {
                Items = entities,
                PageSize = pageSize,
                HasNextPage = hasNextPage,
                NextPageToken = hasNextPage ? entities.Last().Id.ToString() : null
            };

            logger.LogInformation("查詢游標分頁資料成功，大小: {PageSize}, 起始ID: {StartId} - TraceId: {TraceId}", 
                pageSize, startId, traceContext?.TraceId);

            return Result.Success<CursorPaginatedList<Get{{ENTITY}}Response>, Failure>(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "查詢游標分頁資料時發生錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<CursorPaginatedList<Get{{ENTITY}}Response>, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "查詢游標分頁資料時發生錯誤",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    public async Task<Result<bool, Failure>> DeleteAsync(
        int id, 
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        try
        {
            var entity = await dbContext.{{ENTITY}}s
                .FirstOrDefaultAsync(x => x.Id == id, cancel);

            if (entity == null)
            {
                return Result.Failure<bool, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "找不到指定的 {{ENTITY}}",
                    TraceId = traceContext?.TraceId
                });
            }

            dbContext.{{ENTITY}}s.Remove(entity);
            await dbContext.SaveChangesAsync(cancel);

            // 清除相關快取
            await InvalidateRelatedCacheAsync(id);

            logger.LogInformation("成功刪除 {{ENTITY}}: {EntityId} - TraceId: {TraceId}", 
                id, traceContext?.TraceId);

            return Result.Success<bool, Failure>(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除 {{ENTITY}} 時發生錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "刪除資料時發生錯誤",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    public async Task<Result<List<{{ENTITY}}>, Failure>> SearchAsync(
        string keyword, 
        CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        
        try
        {
            var entities = await dbContext.{{ENTITY}}s
                .AsNoTracking()
                .Where(x => x.Name.Contains(keyword))
                // TODO: 加入其他搜尋條件
                .OrderBy(x => x.Name)
                .ToListAsync(cancel);

            logger.LogInformation("搜尋 {{ENTITY}} 完成，關鍵字: {Keyword}, 結果數: {Count} - TraceId: {TraceId}", 
                keyword, entities.Count, traceContext?.TraceId);

            return Result.Success<List<{{ENTITY}}>, Failure>(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜尋 {{ENTITY}} 時發生錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            return Result.Failure<List<{{ENTITY}}>, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "搜尋資料時發生錯誤",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    private async Task InvalidateRelatedCacheAsync(int entityId)
    {
        // 清除特定實體的快取
        var entityCacheKey = $"{CacheKeyPrefix}{entityId}";
        await cache.RemoveAsync(entityCacheKey);

        // 清除分頁快取 (簡化版本，實際可能需要更精細的快取管理)
        var cacheKeys = new[]
        {
            $"{CacheKeyPrefix}offset_*",
            $"{CacheKeyPrefix}cursor_*"
        };

        foreach (var pattern in cacheKeys)
        {
            // 注意：這裡需要快取提供者支援模式匹配刪除
            // 或者使用快取標籤功能
            await cache.RemoveByPatternAsync(pattern);
        }
    }
}