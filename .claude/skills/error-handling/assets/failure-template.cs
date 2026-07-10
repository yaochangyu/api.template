using CSharpFunctionalExtensions;

namespace JobBank1111.Job.WebAPI;

/// <summary>
/// 統一的錯誤回應物件
/// 不序列化 Exception 到客戶端（僅用於追蹤與日誌）
/// </summary>
public class Failure
{
    /// <summary>
    /// 錯誤代碼（對應 FailureCode enum）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 使用者可見的錯誤訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 請求追蹤 ID（用於日誌關聯）
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 原始異常（不序列化到客戶端，僅用於內部追蹤）
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Exception? Exception { get; set; }

    /// <summary>
    /// 額外結構化資料（如驗證錯誤詳情）
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// 錯誤代碼列舉
/// 對應 HTTP 狀態碼：Unauthorized (401), BadRequest (400), NotFound (404), 
/// Conflict (409), InternalServerError (500)
/// </summary>
public enum FailureCode
{
    /// <summary>未授權存取 → 401</summary>
    Unauthorized,

    /// <summary>驗證錯誤 → 400</summary>
    ValidationError,

    /// <summary>重複資源 → 409</summary>
    DuplicateEmail,

    /// <summary>資源不存在 → 404</summary>
    NotFound,

    /// <summary>資料庫錯誤 → 500</summary>
    DbError,

    /// <summary>併發衝突 → 409</summary>
    DbConcurrency,

    /// <summary>無效操作 → 400</summary>
    InvalidOperation,

    /// <summary>逾時 → 408</summary>
    Timeout,

    /// <summary>內部伺服器錯誤 → 500</summary>
    InternalServerError,

    /// <summary>未知錯誤 → 500</summary>
    Unknown
}

/// <summary>
/// 錯誤代碼映射至 HTTP 狀態碼
/// </summary>
public class FailureCodeMapper
{
    public (int StatusCode, object ErrorResponse) Map(Failure failure)
    {
        var statusCode = failure.Code switch
        {
            nameof(FailureCode.Unauthorized) => StatusCodes.Status401Unauthorized,
            nameof(FailureCode.ValidationError) => StatusCodes.Status400BadRequest,
            nameof(FailureCode.DuplicateEmail) => StatusCodes.Status409Conflict,
            nameof(FailureCode.NotFound) => StatusCodes.Status404NotFound,
            nameof(FailureCode.DbConcurrency) => StatusCodes.Status409Conflict,
            nameof(FailureCode.InvalidOperation) => StatusCodes.Status400BadRequest,
            nameof(FailureCode.Timeout) => StatusCodes.Status408RequestTimeout,
            nameof(FailureCode.DbError) => StatusCodes.Status500InternalServerError,
            nameof(FailureCode.InternalServerError) => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        return (statusCode, new
        {
            code = failure.Code,
            message = failure.Message,
            traceId = failure.TraceId,
            data = failure.Data
        });
    }
}

// ============ 使用範例 ============

// 在 Repository 中
public class MemberRepository
{
    public async Task<Result<Member, Failure>> GetByIdAsync(Guid id)
    {
        try
        {
            var member = await _db.Members.FindAsync(id);
            if (member is null)
                return Result.Failure<Member, Failure>(new Failure
                {
                    Code = nameof(FailureCode.NotFound),
                    Message = $"Member {id} not found",
                    TraceId = _context.TraceId
                });
            return Result.Success<Member, Failure>(member);
        }
        catch (Exception ex)
        {
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Database error",
                TraceId = _context.TraceId,
                Exception = ex  // ⭐ 保存原始異常
            });
        }
    }
}

// 在 Handler 中
public class MemberHandler
{
    public async Task<Result<MemberResponse, Failure>> CreateAsync(
        CreateMemberRequest request)
    {
        // 驗證
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Result.Failure<MemberResponse, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Validation failed",
                Data = new Dictionary<string, object>
                {
                    {
                        "errors",
                        validationResult.Errors.ToDictionary(
                            e => e.PropertyName,
                            e => (object)e.ErrorMessage)
                    }
                }
            });

        // 呼叫 Repository 並轉發結果
        var member = new Member { Email = request.Email, ... };
        var insertResult = await _repo.InsertAsync(member);
        
        if (insertResult.IsFailure)
            return Result.Failure<MemberResponse, Failure>(insertResult.Error);

        return Result.Success<MemberResponse, Failure>(
            new MemberResponse { Id = member.Id, ... });
    }
}

// 在 Controller 中
[ApiController]
[Route("api/v1/members")]
public class MemberControllerImpl
{
    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create(
        [FromBody] CreateMemberRequest request)
    {
        var result = await _handler.CreateAsync(request);
        
        if (result.IsSuccess)
            return Created(nameof(GetById), result.Value);

        var (statusCode, errorResponse) = _mapper.Map(result.Error);
        return StatusCode(statusCode, errorResponse);
    }
}
