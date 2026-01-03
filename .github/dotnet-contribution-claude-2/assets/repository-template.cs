using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Feature} Repository 介面
/// </summary>
public interface I{Feature}Repository
{
    Task<Result<{Feature}, Failure>> GetByIdAsync(Guid id, CancellationToken cancel = default);
    Task<Result<PagedData<{Feature}>, Failure>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancel = default);
    Task<Result<{Feature}, Failure>> CreateAsync({Feature} entity, CancellationToken cancel = default);
    Task<Result<{Feature}, Failure>> UpdateAsync({Feature} entity, CancellationToken cancel = default);
    Task<Result, Failure> DeleteAsync(Guid id, CancellationToken cancel = default);
}

/// <summary>
/// {Feature} Repository 範本
/// 職責：資料存取邏輯、EF Core 操作、資料庫查詢封裝
/// </summary>
public class {Feature}Repository(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IContextGetter<TraceContext> contextGetter,
    ILogger<{Feature}Repository> logger) : I{Feature}Repository
{
    /// <summary>
    /// 根據 ID 取得 {Feature}
    /// </summary>
    public async Task<Result<{Feature}, Failure>> GetByIdAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var entity = await dbContext.{Feature}s
                .AsNoTracking()  // ✅ 唯讀查詢使用 AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancel);

            if (entity == null)
            {
                return Result.Failure<{Feature}, Failure>(new Failure
                {
                    Code = FailureCode.NotFound,
                    Message = $"{nameof({Feature})} {id} 不存在",
                    TraceId = traceContext?.TraceId
                });
            }

            return Result.Success<{Feature}, Failure>(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "取得 {Feature} 失敗 - ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), id, traceContext?.TraceId);

            return Result.Failure<{Feature}, Failure>(new Failure
            {
                Code = FailureCode.DbError,
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex  // ⚠️ 必須保存原始例外
            });
        }
    }

    /// <summary>
    /// 取得分頁資料
    /// </summary>
    public async Task<Result<PagedData<{Feature}>, Failure>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            // 計算總筆數
            var totalCount = await dbContext.{Feature}s.CountAsync(cancel);

            // 取得分頁資料
            var items = await dbContext.{Feature}s
                .AsNoTracking()
                .OrderBy(e => e.Name)  // 排序
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(cancel);

            var pagedData = new PagedData<{Feature}>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return Result.Success<PagedData<{Feature}>, Failure>(pagedData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "取得 {Feature} 分頁失敗 - PageIndex: {PageIndex}, PageSize: {PageSize}, TraceId: {TraceId}",
                nameof({Feature}), pageIndex, pageSize, traceContext?.TraceId);

            return Result.Failure<PagedData<{Feature}>, Failure>(new Failure
            {
                Code = FailureCode.DbError,
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 建立 {Feature}
    /// </summary>
    public async Task<Result<{Feature}, Failure>> CreateAsync(
        {Feature} entity,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            dbContext.{Feature}s.Add(entity);
            await dbContext.SaveChangesAsync(cancel);

            logger.LogInformation(
                "{Feature} 建立成功 - ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), entity.Id, traceContext?.TraceId);

            return Result.Success<{Feature}, Failure>(entity);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
        {
            // 重複鍵錯誤（Unique Constraint Violation）
            logger.LogWarning(ex,
                "{Feature} 建立失敗 - 重複鍵錯誤, TraceId: {TraceId}",
                nameof({Feature}), traceContext?.TraceId);

            return Result.Failure<{Feature}, Failure>(new Failure
            {
                Code = FailureCode.DuplicateEmail,  // 或其他適當的錯誤碼
                Message = "資料已存在",
                TraceId = traceContext?.TraceId,
                Exception = ex  // ⚠️ 必須保存原始例外
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // 併發衝突
            logger.LogWarning(ex,
                "{Feature} 建立失敗 - 併發衝突, TraceId: {TraceId}",
                nameof({Feature}), traceContext?.TraceId);

            return Result.Failure<{Feature}, Failure>(new Failure
            {
                Code = FailureCode.DbConcurrency,
                Message = "資料已被其他使用者修改",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
        catch (Exception ex)
        {
            // 其他資料庫錯誤
            logger.LogError(ex,
                "{Feature} 建立失敗 - ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), entity.Id, traceContext?.TraceId);

            return Result.Failure<{Feature}, Failure>(new Failure
            {
                Code = FailureCode.DbError,
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 更新 {Feature}
    /// </summary>
    public async Task<Result<{Feature}, Failure>> UpdateAsync(
        {Feature} entity,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            // 查詢現有實體（需要追蹤）
            var existing = await dbContext.{Feature}s
                .FirstOrDefaultAsync(e => e.Id == entity.Id, cancel);

            if (existing == null)
            {
                return Result.Failure<{Feature}, Failure>(new Failure
                {
                    Code = FailureCode.NotFound,
                    Message = $"{nameof({Feature})} {entity.Id} 不存在",
                    TraceId = traceContext?.TraceId
                });
            }

            // 更新屬性
            dbContext.Entry(existing).CurrentValues.SetValues(entity);

            await dbContext.SaveChangesAsync(cancel);

            logger.LogInformation(
                "{Feature} 更新成功 - ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), entity.Id, traceContext?.TraceId);

            return Result.Success<{Feature}, Failure>(existing);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex,
                "{Feature} 更新失敗 - 併發衝突, ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), entity.Id, traceContext?.TraceId);

            return Result.Failure<{Feature}, Failure>(new Failure
            {
                Code = FailureCode.DbConcurrency,
                Message = "資料已被其他使用者修改",
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "{Feature} 更新失敗 - ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), entity.Id, traceContext?.TraceId);

            return Result.Failure<{Feature}, Failure>(new Failure
            {
                Code = FailureCode.DbError,
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 刪除 {Feature}
    /// </summary>
    public async Task<Result, Failure> DeleteAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var entity = await dbContext.{Feature}s.FindAsync(new object[] { id }, cancel);

            if (entity == null)
            {
                return Result.Failure<Failure>(new Failure
                {
                    Code = FailureCode.NotFound,
                    Message = $"{nameof({Feature})} {id} 不存在",
                    TraceId = traceContext?.TraceId
                });
            }

            dbContext.{Feature}s.Remove(entity);
            await dbContext.SaveChangesAsync(cancel);

            logger.LogInformation(
                "{Feature} 刪除成功 - ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), id, traceContext?.TraceId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "{Feature} 刪除失敗 - ID: {Id}, TraceId: {TraceId}",
                nameof({Feature}), id, traceContext?.TraceId);

            return Result.Failure<Failure>(new Failure
            {
                Code = FailureCode.DbError,
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    // ========================================
    // 進階查詢方法（範例）
    // ========================================

    /// <summary>
    /// 根據條件查詢（範例）
    /// </summary>
    public async Task<Result<List<{Feature}>, Failure>> GetByConditionAsync(
        string searchTerm,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var entities = await dbContext.{Feature}s
                .AsNoTracking()
                .Where(e => e.Name.Contains(searchTerm))  // 條件查詢
                .OrderBy(e => e.Name)
                .ToListAsync(cancel);

            return Result.Success<List<{Feature}>, Failure>(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "查詢 {Feature} 失敗 - SearchTerm: {SearchTerm}, TraceId: {TraceId}",
                nameof({Feature}), searchTerm, traceContext?.TraceId);

            return Result.Failure<List<{Feature}>, Failure>(new Failure
            {
                Code = FailureCode.DbError,
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 批次建立（範例）
    /// </summary>
    public async Task<Result<List<{Feature}>, Failure>> CreateBatchAsync(
        List<{Feature}> entities,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            // ✅ 使用 AddRange 批次新增
            dbContext.{Feature}s.AddRange(entities);
            await dbContext.SaveChangesAsync(cancel);

            logger.LogInformation(
                "{Feature} 批次建立成功 - Count: {Count}, TraceId: {TraceId}",
                nameof({Feature}), entities.Count, traceContext?.TraceId);

            return Result.Success<List<{Feature}>, Failure>(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "{Feature} 批次建立失敗 - Count: {Count}, TraceId: {TraceId}",
                nameof({Feature}), entities.Count, traceContext?.TraceId);

            return Result.Failure<List<{Feature}>, Failure>(new Failure
            {
                Code = FailureCode.DbError,
                Message = ex.Message,
                TraceId = traceContext?.TraceId,
                Exception = ex
            });
        }
    }
}

// ========================================
// 分頁資料 DTO
// ========================================

/// <summary>
/// 分頁資料（用於 Repository 回傳）
/// </summary>
public record PagedData<T>
{
    public required List<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int PageIndex { get; init; }
    public required int PageSize { get; init; }
}
