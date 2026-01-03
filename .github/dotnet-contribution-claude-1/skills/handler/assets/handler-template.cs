using CSharpFunctionalExtensions;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Feature} Handler - 業務邏輯層
/// </summary>
/// <remarks>
/// 職責：
/// - 實作業務規則與驗證
/// - 協調 Repository 進行資料操作
/// - 使用 Result Pattern 處理錯誤
/// - 不包含 HTTP 相關邏輯
/// </remarks>
public class {Feature}Handler(
    {Feature}Repository repository,
    IDbContextFactory<JobBankDbContext> dbContextFactory,
    IContextGetter<TraceContext> traceContextGetter,
    ILogger<{Feature}Handler> logger)
{
    /// <summary>
    /// 建立新的 {Resource}
    /// </summary>
    public async Task<Result<{Resource}Response, Failure>> Create{Resource}Async(
        Create{Resource}Request request,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            // 1. 業務驗證
            var emailExists = await repository.EmailExistsAsync(
                request.Email,
                cancellationToken);

            if (emailExists)
            {
                return Result.Failure<{Resource}Response, Failure>(new Failure
                {
                    Code = nameof(FailureCode.DuplicateEmail),
                    Message = "此 Email 已被註冊",
                    TraceId = traceContext.TraceId
                });
            }

            // 2. 建立實體
            var entity = new {Resource}
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            // 3. 呼叫 Repository
            var createResult = await repository.CreateAsync(entity, cancellationToken);

            // 4. 轉換為 Response
            return createResult.Match(
                success => Result.Success<{Resource}Response, Failure>(
                    MapToResponse(success)),
                failure => Result.Failure<{Resource}Response, Failure>(failure)
            );
        }
        catch (Exception ex)
        {
            return Result.Failure<{Resource}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = ex.Message,
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 根據 ID 查詢 {Resource}
    /// </summary>
    public async Task<Result<{Resource}Response, Failure>> Get{Resource}ByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            var result = await repository.GetByIdAsync(id, cancellationToken);

            return result.Match(
                success => Result.Success<{Resource}Response, Failure>(
                    MapToResponse(success)),
                failure => Result.Failure<{Resource}Response, Failure>(failure)
            );
        }
        catch (Exception ex)
        {
            return Result.Failure<{Resource}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = ex.Message,
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 更新 {Resource}
    /// </summary>
    public async Task<Result<{Resource}Response, Failure>> Update{Resource}Async(
        Guid id,
        Update{Resource}Request request,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            // 1. 查詢現有資料
            var getResult = await repository.GetByIdAsync(id, cancellationToken);
            if (getResult.IsFailure)
                return Result.Failure<{Resource}Response, Failure>(getResult.Error);

            var entity = getResult.Value;

            // 2. 更新欄位
            entity.Name = request.Name;
            entity.UpdatedAt = DateTime.UtcNow;

            // 3. 儲存變更
            var updateResult = await repository.UpdateAsync(entity, cancellationToken);

            return updateResult.Match(
                success => Result.Success<{Resource}Response, Failure>(
                    MapToResponse(success)),
                failure => Result.Failure<{Resource}Response, Failure>(failure)
            );
        }
        catch (Exception ex)
        {
            return Result.Failure<{Resource}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = ex.Message,
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 刪除 {Resource}
    /// </summary>
    public async Task<Result<bool, Failure>> Delete{Resource}Async(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            var result = await repository.DeleteAsync(id, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = ex.Message,
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    /// <summary>
    /// 分頁查詢列表
    /// </summary>
    public async Task<Result<PagedList<{Resource}Response>, Failure>> Get{Resource}ListAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            var result = await repository.GetPagedListAsync(
                pageIndex,
                pageSize,
                cancellationToken);

            return result.Match(
                success => Result.Success<PagedList<{Resource}Response>, Failure>(
                    new PagedList<{Resource}Response>
                    {
                        Items = success.Items.Select(MapToResponse).ToList(),
                        PageIndex = success.PageIndex,
                        PageSize = success.PageSize,
                        TotalCount = success.TotalCount,
                        TotalPages = success.TotalPages
                    }),
                failure => Result.Failure<PagedList<{Resource}Response>, Failure>(failure)
            );
        }
        catch (Exception ex)
        {
            return Result.Failure<PagedList<{Resource}Response>, Failure>(new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = ex.Message,
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }

    // 私有輔助方法
    private static {Resource}Response MapToResponse({Resource} entity)
    {
        return new {Resource}Response
        {
            Id = entity.Id,
            Email = entity.Email,
            Name = entity.Name,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

/* 使用說明
 *
 * 1. 替換佔位符：
 *    {Feature} = 功能名稱（例如：Member）
 *    {Resource} = 資源名稱（例如：Member）
 *
 * 2. 核心原則：
 *    ✅ 使用 Result Pattern
 *    ✅ 所有例外封裝到 Failure.Exception
 *    ✅ 支援 CancellationToken
 *    ✅ 透過 Repository 存取資料
 *    ❌ 不記錄錯誤日誌（由 Middleware 處理）
 *    ❌ 不拋出業務邏輯例外
 *    ❌ 不包含 HTTP 相關邏輯
 */
