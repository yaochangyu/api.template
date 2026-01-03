using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;

namespace JobBank1111.Job.WebAPI.{Feature};

/// <summary>
/// {Feature} Controller 範本
/// 職責：HTTP 請求/回應處理、路由、HTTP 狀態碼對應
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class {Feature}Controller(
    I{Feature}Handler handler,
    ILogger<{Feature}Controller> logger) : ControllerBase
{
    /// <summary>
    /// 取得單一 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>{Feature} 資料</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof({Feature}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancel)
    {
        var result = await handler.GetByIdAsync(id, cancel);

        return result.Match(
            onSuccess: data => Ok(data),
            onFailure: failure =>
            {
                var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);
                return StatusCode(statusCode, failure);
            }
        );
    }

    /// <summary>
    /// 取得 {Feature} 列表（分頁）
    /// </summary>
    /// <param name="pageIndex">頁碼（從 0 開始）</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>{Feature} 列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<{Feature}Response>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancel = default)
    {
        var result = await handler.GetPagedAsync(pageIndex, pageSize, cancel);

        return result.Match(
            onSuccess: data => Ok(data),
            onFailure: failure =>
            {
                var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);
                return StatusCode(statusCode, failure);
            }
        );
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
        CancellationToken cancel)
    {
        var result = await handler.CreateAsync(request, cancel);

        return result.Match(
            onSuccess: data => CreatedAtAction(
                nameof(GetById),
                new { id = data.Id },
                data),
            onFailure: failure =>
            {
                var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);
                return StatusCode(statusCode, failure);
            }
        );
    }

    /// <summary>
    /// 更新 {Feature}
    /// </summary>
    /// <param name="id">{Feature} ID</param>
    /// <param name="request">更新請求</param>
    /// <param name="cancel">取消權杖</param>
    /// <returns>更新的 {Feature} 資料</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof({Feature}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(Failure), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] Update{Feature}Request request,
        CancellationToken cancel)
    {
        // 確保路由 ID 與請求 ID 一致
        if (id != request.Id)
        {
            return BadRequest(new Failure
            {
                Code = FailureCode.ValidationError,
                Message = "路由 ID 與請求 ID 不一致"
            });
        }

        var result = await handler.UpdateAsync(request, cancel);

        return result.Match(
            onSuccess: data => Ok(data),
            onFailure: failure =>
            {
                var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);
                return StatusCode(statusCode, failure);
            }
        );
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
        [FromRoute] Guid id,
        CancellationToken cancel)
    {
        var result = await handler.DeleteAsync(id, cancel);

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: failure =>
            {
                var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);
                return StatusCode(statusCode, failure);
            }
        );
    }
}

// ========================================
// DTO 定義（放在相同檔案或獨立檔案）
// ========================================

/// <summary>
/// {Feature} 回應 DTO
/// </summary>
public record {Feature}Response
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    // 其他屬性...
}

/// <summary>
/// 建立 {Feature} 請求 DTO
/// </summary>
public record Create{Feature}Request
{
    public required string Name { get; init; }
    // 其他屬性...
}

/// <summary>
/// 更新 {Feature} 請求 DTO
/// </summary>
public record Update{Feature}Request
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    // 其他屬性...
}

/// <summary>
/// 分頁結果 DTO
/// </summary>
public record PagedResult<T>
{
    public required List<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int PageIndex { get; init; }
    public required int PageSize { get; init; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex < TotalPages - 1;
}
