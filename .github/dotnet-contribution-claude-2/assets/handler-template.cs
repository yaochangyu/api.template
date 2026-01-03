using CSharpFunctionalExtensions;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Feature} Handler 介面
/// </summary>
public interface I{Feature}Handler
{
    Task<Result<{Feature}Response, Failure>> GetByIdAsync(Guid id, CancellationToken cancel = default);
    Task<Result<PagedResult<{Feature}Response>, Failure>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancel = default);
    Task<Result<{Feature}Response, Failure>> CreateAsync(Create{Feature}Request request, CancellationToken cancel = default);
    Task<Result<{Feature}Response, Failure>> UpdateAsync(Update{Feature}Request request, CancellationToken cancel = default);
    Task<Result, Failure> DeleteAsync(Guid id, CancellationToken cancel = default);
}

/// <summary>
/// {Feature} Handler 範本
/// 職責：核心業務邏輯、流程協調、錯誤處理與結果封裝
/// </summary>
public class {Feature}Handler(
    I{Feature}Repository repository,
    IContextGetter<TraceContext> contextGetter,
    ILogger<{Feature}Handler> logger) : I{Feature}Handler
{
    /// <summary>
    /// 取得單一 {Feature}
    /// </summary>
    public async Task<Result<{Feature}Response, Failure>> GetByIdAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        // 呼叫 Repository
        var result = await repository.GetByIdAsync(id, cancel);

        // 處理失敗情況
        if (result.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(result.Error);
        }

        // 轉換為 Response DTO
        var entity = result.Value;
        var response = new {Feature}Response
        {
            Id = entity.Id,
            Name = entity.Name
            // 其他屬性映射...
        };

        return Result.Success<{Feature}Response, Failure>(response);
    }

    /// <summary>
    /// 取得 {Feature} 列表（分頁）
    /// </summary>
    public async Task<Result<PagedResult<{Feature}Response>, Failure>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        // 驗證分頁參數
        if (pageIndex < 0 || pageSize <= 0 || pageSize > 100)
        {
            return Result.Failure<PagedResult<{Feature}Response>, Failure>(new Failure
            {
                Code = FailureCode.ValidationError,
                Message = "分頁參數不正確：pageIndex 必須 >= 0，pageSize 必須在 1-100 之間",
                TraceId = traceContext?.TraceId
            });
        }

        // 呼叫 Repository
        var result = await repository.GetPagedAsync(pageIndex, pageSize, cancel);

        if (result.IsFailure)
        {
            return Result.Failure<PagedResult<{Feature}Response>, Failure>(result.Error);
        }

        var pagedData = result.Value;

        // 轉換為 Response DTO
        var items = pagedData.Items.Select(entity => new {Feature}Response
        {
            Id = entity.Id,
            Name = entity.Name
            // 其他屬性映射...
        }).ToList();

        var response = new PagedResult<{Feature}Response>
        {
            Items = items,
            TotalCount = pagedData.TotalCount,
            PageIndex = pagedData.PageIndex,
            PageSize = pagedData.PageSize
        };

        return Result.Success<PagedResult<{Feature}Response>, Failure>(response);
    }

    /// <summary>
    /// 建立新的 {Feature}
    /// </summary>
    public async Task<Result<{Feature}Response, Failure>> CreateAsync(
        Create{Feature}Request request,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        // ========================================
        // 業務邏輯驗證
        // ========================================
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<{Feature}Response, Failure>(new Failure
            {
                Code = FailureCode.ValidationError,
                Message = "Name 不可為空",
                TraceId = traceContext?.TraceId,
                Data = new Dictionary<string, object> { ["Field"] = "Name" }
            });
        }

        // 其他業務規則驗證...

        // ========================================
        // 建立實體
        // ========================================
        var entity = new {Feature}
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = traceContext?.UserId
            // 其他屬性...
        };

        // ========================================
        // 呼叫 Repository
        // ========================================
        var result = await repository.CreateAsync(entity, cancel);

        if (result.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(result.Error);
        }

        // ========================================
        // 轉換為 Response DTO
        // ========================================
        var createdEntity = result.Value;
        var response = new {Feature}Response
        {
            Id = createdEntity.Id,
            Name = createdEntity.Name
            // 其他屬性映射...
        };

        logger.LogInformation(
            "{Feature} 建立成功 - ID: {Id}, TraceId: {TraceId}",
            nameof({Feature}),
            response.Id,
            traceContext?.TraceId);

        return Result.Success<{Feature}Response, Failure>(response);
    }

    /// <summary>
    /// 更新 {Feature}
    /// </summary>
    public async Task<Result<{Feature}Response, Failure>> UpdateAsync(
        Update{Feature}Request request,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        // ========================================
        // 業務邏輯驗證
        // ========================================
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<{Feature}Response, Failure>(new Failure
            {
                Code = FailureCode.ValidationError,
                Message = "Name 不可為空",
                TraceId = traceContext?.TraceId,
                Data = new Dictionary<string, object> { ["Field"] = "Name" }
            });
        }

        // ========================================
        // 查詢現有實體
        // ========================================
        var existingResult = await repository.GetByIdAsync(request.Id, cancel);

        if (existingResult.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(existingResult.Error);
        }

        var existing = existingResult.Value;

        // ========================================
        // 更新屬性
        // ========================================
        existing.Name = request.Name;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = traceContext?.UserId;
        // 其他屬性更新...

        // ========================================
        // 呼叫 Repository 儲存
        // ========================================
        var result = await repository.UpdateAsync(existing, cancel);

        if (result.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(result.Error);
        }

        // ========================================
        // 轉換為 Response DTO
        // ========================================
        var updatedEntity = result.Value;
        var response = new {Feature}Response
        {
            Id = updatedEntity.Id,
            Name = updatedEntity.Name
            // 其他屬性映射...
        };

        logger.LogInformation(
            "{Feature} 更新成功 - ID: {Id}, TraceId: {TraceId}",
            nameof({Feature}),
            response.Id,
            traceContext?.TraceId);

        return Result.Success<{Feature}Response, Failure>(response);
    }

    /// <summary>
    /// 刪除 {Feature}
    /// </summary>
    public async Task<Result, Failure> DeleteAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        var traceContext = contextGetter.Context;

        // ========================================
        // 檢查實體是否存在
        // ========================================
        var existingResult = await repository.GetByIdAsync(id, cancel);

        if (existingResult.IsFailure)
        {
            return Result.Failure<Failure>(existingResult.Error);
        }

        // ========================================
        // 業務邏輯檢查（是否可刪除）
        // ========================================
        // 例如：檢查是否有關聯資料
        // if (existing.HasRelatedData)
        // {
        //     return Result.Failure<Failure>(new Failure
        //     {
        //         Code = FailureCode.InvalidOperation,
        //         Message = "無法刪除，存在關聯資料",
        //         TraceId = traceContext?.TraceId
        //     });
        // }

        // ========================================
        // 呼叫 Repository 刪除
        // ========================================
        var result = await repository.DeleteAsync(id, cancel);

        if (result.IsFailure)
        {
            return Result.Failure<Failure>(result.Error);
        }

        logger.LogInformation(
            "{Feature} 刪除成功 - ID: {Id}, TraceId: {TraceId}",
            nameof({Feature}),
            id,
            traceContext?.TraceId);

        return Result.Success();
    }

    // ========================================
    // 私有輔助方法（可選）
    // ========================================

    /// <summary>
    /// 驗證業務規則（範例）
    /// </summary>
    private Result<Failure> ValidateBusinessRules({Feature} entity)
    {
        // 業務規則驗證邏輯
        // 例如：檢查名稱長度、格式等

        if (entity.Name.Length > 100)
        {
            return Result.Failure<Failure>(new Failure
            {
                Code = FailureCode.ValidationError,
                Message = "Name 長度不可超過 100 個字元"
            });
        }

        return Result.Success();
    }
}
