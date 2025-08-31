// JobBank1111 API Model 範本庫
// 基於 MemberRepository 分析，以下是標準的 Request/Response Model 範本

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI.{Entity};

// Insert Models

/// <summary>
/// Insert Request Model - 新增實體請求模型
/// </summary>
public class Insert{Entity}Request
{
    [Required(ErrorMessage = "Email 為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的 Email 格式")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Name 為必填欄位")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name 長度需介於 2-100 字元")]
    public string Name { get; set; }

    [Range(1, 150, ErrorMessage = "Age 需介於 1-150 之間")]
    public int Age { get; set; }
}

/// <summary>
/// Insert Response Model - 新增實體回應模型
/// </summary>
public class Insert{Entity}Response
{
    public int AffectedRows { get; set; }
    public string Message { get; set; }
}

// Get By Email Models

/// <summary>
/// Get By Email Request Model - 透過 Email 查詢請求模型
/// </summary>
public class Get{Entity}ByEmailRequest
{
    [Required(ErrorMessage = "Email 為必填欄位")]
    [EmailAddress(ErrorMessage = "請輸入有效的 Email 格式")]
    public string Email { get; set; }
}

/// <summary>
/// Get By Email Response Model - 透過 Email 查詢回應模型
/// </summary>
public class Get{Entity}ByEmailResponse
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string Email { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
}

// Offset Pagination Models

/// <summary>
/// Offset Pagination Request Model - Offset 分頁請求模型
/// </summary>
public class Get{Entity}sOffsetRequest
{
    [Range(0, int.MaxValue, ErrorMessage = "PageIndex 不能小於 0")]
    public int PageIndex { get; set; } = 0;

    [Range(1, 100, ErrorMessage = "PageSize 需介於 1-100 之間")]
    public int PageSize { get; set; } = 10;

    public bool NoCache { get; set; } = false;
}

/// <summary>
/// Offset Pagination Response Model - Offset 分頁回應模型
/// </summary>
public class Get{Entity}sOffsetResponse
{
    public List<Get{Entity}Response> Items { get; set; } = new();
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}

// Cursor Pagination Models

/// <summary>
/// Cursor Pagination Request Model - Cursor 分頁請求模型
/// </summary>
public class Get{Entity}sCursorRequest
{
    [Range(1, 100, ErrorMessage = "PageSize 需介於 1-100 之間")]
    public int PageSize { get; set; } = 10;

    public string? NextPageToken { get; set; }

    public bool NoCache { get; set; } = true;
}

/// <summary>
/// Cursor Pagination Response Model - Cursor 分頁回應模型
/// </summary>
public class Get{Entity}sCursorResponse
{
    public List<Get{Entity}Response> Items { get; set; } = new();
    public string? NextPageToken { get; set; }
    public string? PreviousPageToken { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

// Standard Response Model

/// <summary>
/// Standard Get Response Model - 標準取得回應模型
/// </summary>
public class Get{Entity}Response
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string Email { get; set; }

    [JsonIgnore]
    public long? SequenceId { get; set; }
}

// Command Class Template

/// <summary>
/// Command Class Template - 命令類別範本
/// 基於 MemberCommand 模式設計
/// </summary>
public class {Entity}Command(
    {Entity}Repository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<{Entity}Command> logger)
{
    /// <summary>
    /// 新增實體 - 包含 Email 重複檢查
    /// </summary>
    public async Task<Result<Insert{Entity}Response, Failure>>
        Insert{Entity}Async(Insert{Entity}Request request,
                            CancellationToken cancel = default)
    {
        var traceContext = traceContextGetter.Get();
        logger.LogInformation("開始新增{Entity} - Email: {Email}, TraceId: {TraceId}", 
            request.Email, traceContext?.TraceId);

        // 檢查 Email 是否重複
        var queryResult = await repository.QueryEmailAsync(request.Email, cancel);
        if (queryResult.IsFailure)
        {
            logger.LogError("查詢{Entity}失敗 - Email: {Email}, Error: {Error}", 
                request.Email, queryResult.Error.Message);
            return Result.Failure<Insert{Entity}Response, Failure>(queryResult.Error);
        }

        var existing{Entity} = queryResult.Value;
        if (existing{Entity} != null)
        {
            var failure = new Failure
            {
                Code = nameof(FailureCode.DuplicateEmail),
                Message = "Email 已存在",
                Data = new { Email = request.Email },
                TraceId = traceContext?.TraceId
            };
            logger.LogWarning("{Entity} Email 重複 - Email: {Email}", request.Email);
            return Result.Failure<Insert{Entity}Response, Failure>(failure);
        }

        var insertResult = await repository.InsertAsync(request, cancel);
        if (insertResult.IsFailure)
        {
            logger.LogError("新增{Entity}失敗 - Email: {Email}, Error: {Error}", 
                request.Email, insertResult.Error.Message);
            return Result.Failure<Insert{Entity}Response, Failure>(insertResult.Error);
        }

        var response = new Insert{Entity}Response
        {
            AffectedRows = insertResult.Value,
            Message = "{Entity}新增成功"
        };

        logger.LogInformation("{Entity}新增成功 - Email: {Email}, AffectedRows: {AffectedRows}", 
            request.Email, insertResult.Value);

        return Result.Success<Insert{Entity}Response, Failure>(response);
    }

    // 其他方法: Get{Entity}ByEmailAsync, Get{Entity}sOffsetAsync, Get{Entity}sCursorAsync
    // 遵循相同的模式...
}

/* 
設計原則:
1. 根據需求建立多種 Request/Response Model:
2. **命名規範**: 遵循專案的命名慣例
3. **可空性**: 適當使用可空類型
4. **JSON 序列化**: 支援 System.Text.Json 序列化
5. **分頁支援**: 提供 Offset 和 Cursor 兩種分頁模式
6. **錯誤處理**: 整合 Result Pattern 和 Failure 物件
7. **追蹤整合**: 自動整合 TraceContext 追蹤資訊

使用方式:
- 將檔案中的 {Entity} 替換為實際的實體名稱 (如 Member, Product, Order)
- 根據實際需求調整屬性和驗證規則
- 遵循專案的命名空間結構
*/