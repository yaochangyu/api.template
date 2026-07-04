using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Entity} Repository - 資料表導向
/// </summary>
/// <remarks>
/// 適用場景：
/// - 簡單 CRUD 操作
/// - 單一資料表
/// - 無複雜業務邏輯
/// - 不需要跨表操作
/// </remarks>
public class {Entity}Repository(
    IDbContextFactory<JobBankDbContext> dbContextFactory,
    IContextGetter<TraceContext> traceContextGetter)
{
    /// <summary>
    /// 建立新實體
    /// </summary>
    public async Task<Result<{Entity}, Failure>> CreateAsync(
        {Entity} entity,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            dbContext.{Entities}.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success<{Entity}, Failure>(entity);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<{Entity}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "資料庫操作失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 根據 ID 查詢
    /// </summary>
    public async Task<Result<{Entity}, Failure>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await dbContext.{Entities}
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity == null)
            {
                return Result.Failure<{Entity}, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "{Entity} 不存在",
                    TraceId = traceContext.TraceId
                });
            }

            return Result.Success<{Entity}, Failure>(entity);
        }
        catch (Exception ex)
        {
            return Result.Failure<{Entity}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "查詢失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 更新實體
    /// </summary>
    public async Task<Result<{Entity}, Failure>> UpdateAsync(
        {Entity} entity,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            dbContext.{Entities}.Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success<{Entity}, Failure>(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Result.Failure<{Entity}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbConcurrency),
                Message = "資料已被其他使用者修改",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<{Entity}, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "更新失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 刪除實體
    /// </summary>
    public async Task<Result<bool, Failure>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var entity = await dbContext.{Entities}
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity == null)
            {
                return Result.Failure<bool, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = "{Entity} 不存在",
                    TraceId = traceContext.TraceId
                });
            }

            dbContext.{Entities}.Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success<bool, Failure>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "刪除失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 分頁查詢
    /// </summary>
    public async Task<Result<PagedList<{Entity}>, Failure>> GetPagedListAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var totalCount = await dbContext.{Entities}.CountAsync(cancellationToken);

            var items = await dbContext.{Entities}
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var pagedList = new PagedList<{Entity}>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Result.Success<PagedList<{Entity}>, Failure>(pagedList);
        }
        catch (Exception ex)
        {
            return Result.Failure<PagedList<{Entity}>, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "查詢列表失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 檢查 Email 是否存在
    /// </summary>
    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.{Entities}
            .AsNoTracking()
            .AnyAsync(e => e.Email == email, cancellationToken);
    }
}

/* 使用說明
 *
 * 1. 替換佔位符：
 *    {Entity} = 實體名稱（單數，例如：Member）
 *    {Entities} = 實體名稱（複數，例如：Members）
 *    {Feature} = 功能名稱（例如：Member）
 *
 * 2. 核心原則：
 *    ✅ 單一資料表操作
 *    ✅ 使用 DbContextFactory 模式
 *    ✅ 使用 Result Pattern
 *    ✅ AsNoTracking for 唯讀查詢
 *    ✅ 支援 CancellationToken
 */
