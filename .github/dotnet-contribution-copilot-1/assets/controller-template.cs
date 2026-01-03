using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Feature} API Controller
/// </summary>
/// <remarks>
/// 此範本展示 Controller 層的標準實作模式
/// 
/// 職責：
/// - HTTP 請求/回應處理
/// - 路由定義
/// - 請求參數驗證
/// - Result Pattern 回應轉換
/// - HTTP 狀態碼對應
/// 
/// 不應該做：
/// - ❌ 業務邏輯處理（交給 Handler）
/// - ❌ 直接存取資料庫（交給 Repository）
/// - ❌ 錯誤日誌記錄（交給 Middleware）
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class {Feature}Controller(
    {Feature}Handler handler,
    IContextGetter<TraceContext> traceContextGetter) : ControllerBase
{
    /// <summary>
    /// 查詢 {Feature} 列表（分頁）
    /// </summary>
    /// <param name="pageNumber">頁碼（從 0 開始）</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>分頁資料</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<{Feature}Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancel = default)
    {
        // 簡單驗證
        if (pageNumber < 0 || pageSize <= 0 || pageSize > 100)
        {
            return BadRequest(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Invalid pagination parameters",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        // 呼叫 Handler
        var result = await handler.GetListAsync(pageNumber, pageSize, cancel);

        // 轉換回應
        return result.IsSuccess
            ? Ok(result.Value)
            : ToErrorResponse(result.Error);
    }

    /// <summary>
    /// 根據 ID 查詢單一 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>{Feature} 詳細資料</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof({Feature}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancel = default)
    {
        var result = await handler.GetByIdAsync(id, cancel);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToErrorResponse(result.Error);
    }

    /// <summary>
    /// 建立新的 {Feature}
    /// </summary>
    /// <param name="request">建立請求</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>建立的 {Feature} 資料</returns>
    [HttpPost]
    [ProducesResponseType(typeof({Feature}Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] Create{Feature}Request request,
        CancellationToken cancel = default)
    {
        // FluentValidation 會自動驗證 request
        // 如果驗證失敗，會自動回傳 400 BadRequest

        var result = await handler.CreateAsync(request, cancel);

        if (result.IsSuccess)
        {
            // 201 Created，包含 Location 標頭
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value.Id },
                result.Value);
        }

        return ToErrorResponse(result.Error);
    }

    /// <summary>
    /// 更新現有的 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="request">更新請求</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>更新後的 {Feature} 資料</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof({Feature}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] Update{Feature}Request request,
        CancellationToken cancel = default)
    {
        // ID 一致性檢查
        if (request.Id != id)
        {
            return BadRequest(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "ID in URL does not match ID in request body",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var result = await handler.UpdateAsync(request, cancel);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToErrorResponse(result.Error);
    }

    /// <summary>
    /// 刪除 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>無內容</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancel = default)
    {
        var result = await handler.DeleteAsync(id, cancel);

        return result.IsSuccess
            ? NoContent()
            : ToErrorResponse(result.Error);
    }

    /// <summary>
    /// 將 Failure 轉換為 IActionResult
    /// </summary>
    private IActionResult ToErrorResponse(Failure failure)
    {
        // 加入 TraceId
        var traceContext = traceContextGetter.Get();
        var failureWithTrace = failure with { TraceId = traceContext.TraceId };

        // 映射到 HTTP 狀態碼
        var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);

        return StatusCode(statusCode, failureWithTrace);
    }
}

/// <summary>
/// 建立 {Feature} 請求 DTO
/// </summary>
public record Create{Feature}Request
{
    // TODO: 根據實際需求定義屬性
    // 範例：
    // public string Name { get; init; }
    // public string Email { get; init; }
    // public int Age { get; init; }
}

/// <summary>
/// 更新 {Feature} 請求 DTO
/// </summary>
public record Update{Feature}Request
{
    public Guid Id { get; init; }
    
    // TODO: 根據實際需求定義屬性
    // 範例：
    // public string Name { get; init; }
    // public string Email { get; init; }
    // public int Age { get; init; }
}

/// <summary>
/// {Feature} 回應 DTO
/// </summary>
public record {Feature}Response
{
    public Guid Id { get; init; }
    
    // TODO: 根據實際需求定義屬性
    // 範例：
    // public string Name { get; init; }
    // public string Email { get; init; }
    // public int Age { get; init; }
    // public DateTime CreatedAt { get; init; }
    // public DateTime? UpdatedAt { get; init; }
}

/*
使用方式：

1. 將 {Feature} 替換為實際的功能名稱（例如：Member、Product、Order）
2. 定義 Request 和 Response DTO 的屬性
3. 實作對應的 Handler
4. 註冊服務到 DI 容器

範例：
services.AddScoped<MemberHandler>();

注意事項：
- ✅ 使用主建構函式注入依賴
- ✅ 所有方法都傳遞 CancellationToken
- ✅ 使用 Result Pattern 處理錯誤
- ✅ 透過 FailureCodeMapper 映射狀態碼
- ✅ 加入 TraceId 用於日誌追蹤
- ❌ 不在 Controller 中處理業務邏輯
- ❌ 不直接記錄錯誤日誌
*/
