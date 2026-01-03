using CSharpFunctionalExtensions;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Feature} Handler - 業務邏輯層
/// </summary>
/// <remarks>
/// 此範本展示 Handler 層的標準實作模式
/// 
/// 職責：
/// - 核心業務邏輯處理
/// - 流程協調（呼叫多個 Repository）
/// - 錯誤處理與結果封裝（Result Pattern）
/// - 交易協調
/// 
/// 不應該做：
/// - ❌ HTTP 相關邏輯（交給 Controller）
/// - ❌ 直接處理 Request/Response 物件
/// - ❌ 記錄錯誤日誌（只回傳 Failure，由 Middleware 記錄）
/// </remarks>
public class {Feature}Handler(
    {Feature}Repository repository,
    IContextGetter<TraceContext> traceContextGetter)
{
    /// <summary>
    /// 查詢 {Feature} 列表（分頁）
    /// </summary>
    /// <param name="pageNumber">頁碼（從 0 開始）</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>分頁資料或錯誤</returns>
    public async Task<Result<List<{Feature}Response>, Failure>> GetListAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancel = default)
    {
        // 業務邏輯驗證
        if (pageNumber < 0)
        {
            return Result.Failure<List<{Feature}Response>, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Page number cannot be negative"
            });
        }

        if (pageSize <= 0 || pageSize > 100)
        {
            return Result.Failure<List<{Feature}Response>, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Page size must be between 1 and 100"
            });
        }

        // 呼叫 Repository 查詢資料
        var result = await repository.GetPageAsync(pageNumber, pageSize, cancel);

        // 處理 Repository 回傳的錯誤
        if (result.IsFailure)
        {
            return Result.Failure<List<{Feature}Response>, Failure>(result.Error);
        }

        // 將實體轉換為 DTO
        var response = result.Value.Select(entity => new {Feature}Response
        {
            Id = entity.Id,
            // TODO: 映射其他屬性
            // Name = entity.Name,
            // Email = entity.Email,
        }).ToList();

        return Result.Success<List<{Feature}Response>, Failure>(response);
    }

    /// <summary>
    /// 根據 ID 查詢單一 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>{Feature} 資料或錯誤</returns>
    public async Task<Result<{Feature}Response, Failure>> GetByIdAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        // 業務邏輯驗證
        if (id == Guid.Empty)
        {
            return Result.Failure<{Feature}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "ID cannot be empty"
            });
        }

        // 呼叫 Repository
        var result = await repository.GetByIdAsync(id, cancel);

        if (result.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(result.Error);
        }

        // 檢查是否存在
        if (result.Value == null)
        {
            return Result.Failure<{Feature}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.NotFound),
                Message = $"{Feature} with ID {id} not found"
            });
        }

        // 轉換為 DTO
        var response = new {Feature}Response
        {
            Id = result.Value.Id,
            // TODO: 映射其他屬性
        };

        return Result.Success<{Feature}Response, Failure>(response);
    }

    /// <summary>
    /// 建立新的 {Feature}
    /// </summary>
    /// <param name="request">建立請求</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>建立的 {Feature} 資料或錯誤</returns>
    public async Task<Result<{Feature}Response, Failure>> CreateAsync(
        Create{Feature}Request request,
        CancellationToken cancel = default)
    {
        // 業務邏輯驗證
        // TODO: 根據實際需求添加驗證邏輯
        // 範例：
        // if (string.IsNullOrWhiteSpace(request.Email))
        // {
        //     return Result.Failure<{Feature}Response, Failure>(new Failure
        //     {
        //         Code = nameof(FailureCode.ValidationError),
        //         Message = "Email cannot be empty"
        //     });
        // }

        // 檢查重複（範例）
        // var existsResult = await repository.ExistsByEmailAsync(request.Email, cancel);
        // if (existsResult.IsSuccess && existsResult.Value)
        // {
        //     return Result.Failure<{Feature}Response, Failure>(new Failure
        //     {
        //         Code = nameof(FailureCode.DuplicateEmail),
        //         Message = $"Email {request.Email} already exists"
        //     });
        // }

        // 建立實體
        var entity = new {Feature}Entity
        {
            Id = Guid.NewGuid(),
            // TODO: 映射屬性
            // Name = request.Name,
            // Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        // 取得 TraceContext（用於稽核）
        var traceContext = traceContextGetter.Get();
        // entity.CreatedBy = traceContext.UserId;

        // 呼叫 Repository 建立資料
        var result = await repository.CreateAsync(entity, cancel);

        if (result.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(result.Error);
        }

        // 轉換為 DTO
        var response = new {Feature}Response
        {
            Id = result.Value.Id,
            // TODO: 映射其他屬性
        };

        return Result.Success<{Feature}Response, Failure>(response);
    }

    /// <summary>
    /// 更新現有的 {Feature}
    /// </summary>
    /// <param name="request">更新請求</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>更新後的 {Feature} 資料或錯誤</returns>
    public async Task<Result<{Feature}Response, Failure>> UpdateAsync(
        Update{Feature}Request request,
        CancellationToken cancel = default)
    {
        // 業務邏輯驗證
        if (request.Id == Guid.Empty)
        {
            return Result.Failure<{Feature}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "ID cannot be empty"
            });
        }

        // 檢查是否存在
        var existingResult = await repository.GetByIdAsync(request.Id, cancel);
        
        if (existingResult.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(existingResult.Error);
        }

        if (existingResult.Value == null)
        {
            return Result.Failure<{Feature}Response, Failure>(new Failure
            {
                Code = nameof(FailureCode.NotFound),
                Message = $"{Feature} with ID {request.Id} not found"
            });
        }

        // 更新實體
        var entity = existingResult.Value;
        // TODO: 更新屬性
        // entity.Name = request.Name;
        // entity.Email = request.Email;
        entity.UpdatedAt = DateTime.UtcNow;

        // 取得 TraceContext（用於稽核）
        var traceContext = traceContextGetter.Get();
        // entity.UpdatedBy = traceContext.UserId;

        // 呼叫 Repository 更新資料
        var result = await repository.UpdateAsync(entity, cancel);

        if (result.IsFailure)
        {
            return Result.Failure<{Feature}Response, Failure>(result.Error);
        }

        // 轉換為 DTO
        var response = new {Feature}Response
        {
            Id = result.Value.Id,
            // TODO: 映射其他屬性
        };

        return Result.Success<{Feature}Response, Failure>(response);
    }

    /// <summary>
    /// 刪除 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>成功或錯誤</returns>
    public async Task<Result<bool, Failure>> DeleteAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        // 業務邏輯驗證
        if (id == Guid.Empty)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "ID cannot be empty"
            });
        }

        // 檢查是否存在
        var existsResult = await repository.GetByIdAsync(id, cancel);
        
        if (existsResult.IsFailure)
        {
            return Result.Failure<bool, Failure>(existsResult.Error);
        }

        if (existsResult.Value == null)
        {
            return Result.Failure<bool, Failure>(new Failure
            {
                Code = nameof(FailureCode.NotFound),
                Message = $"{Feature} with ID {id} not found"
            });
        }

        // 業務邏輯檢查（範例：檢查是否可以刪除）
        // if (existsResult.Value.HasRelatedData)
        // {
        //     return Result.Failure<bool, Failure>(new Failure
        //     {
        //         Code = nameof(FailureCode.InvalidOperation),
        //         Message = "Cannot delete {Feature} with related data"
        //     });
        // }

        // 呼叫 Repository 刪除資料
        var result = await repository.DeleteAsync(id, cancel);

        return result.IsSuccess
            ? Result.Success<bool, Failure>(true)
            : Result.Failure<bool, Failure>(result.Error);
    }
}

/// <summary>
/// {Feature} 實體（範例）
/// </summary>
/// <remarks>
/// 實際實體可能定義在 DB 專案中
/// </remarks>
public class {Feature}Entity
{
    public Guid Id { get; set; }
    
    // TODO: 根據實際需求定義屬性
    // public string Name { get; set; }
    // public string Email { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // 稽核欄位（可選）
    // public string CreatedBy { get; set; }
    // public string UpdatedBy { get; set; }
}

/*
使用方式：

1. 將 {Feature} 替換為實際的功能名稱（例如：Member、Product、Order）
2. 實作業務邏輯驗證
3. 實作實體到 DTO 的映射
4. 註冊服務到 DI 容器

範例：
services.AddScoped<MemberHandler>();

注意事項：
- ✅ 使用主建構函式注入依賴
- ✅ 所有方法回傳 Result<T, Failure>
- ✅ 傳遞 CancellationToken
- ✅ 使用 nameof(FailureCode.*)
- ✅ 從 TraceContext 取得使用者資訊
- ❌ 不處理 HTTP 相關邏輯
- ❌ 不記錄錯誤日誌
- ❌ 不拋出業務邏輯例外
*/
