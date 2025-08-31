using JobBank1111.Job.WebAPI.Contract;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.{{ENTITY}};

/// <summary>
/// {{ENTITY}} Controller 介面定義
/// 注意：這個介面通常是從 OpenAPI 規格自動產生的
/// </summary>
public interface I{{ENTITY}}Controller
{
    Task<ActionResult<Get{{ENTITY}}ResponsePaginatedList>> Get{{ENTITY}}OffsetAsync(CancellationToken cancellationToken = default);
    Task<ActionResult<Get{{ENTITY}}ResponseCursorPaginatedList>> Get{{ENTITY}}CursorAsync(CancellationToken cancellationToken = default);
    Task<ActionResult<Get{{ENTITY}}Response>> Get{{ENTITY}}ByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IActionResult> Create{{ENTITY}}Async(Contract.Create{{ENTITY}}Request body, CancellationToken cancellationToken = default);
    Task<IActionResult> Update{{ENTITY}}Async(int id, Contract.Update{{ENTITY}}Request body, CancellationToken cancellationToken = default);
    Task<IActionResult> Delete{{ENTITY}}Async(int id, CancellationToken cancellationToken = default);
    Task<ActionResult<List<Get{{ENTITY}}Response>>> Search{{ENTITY}}Async(string keyword, CancellationToken cancellationToken = default);
}

public class {{ENTITY}}ControllerImpl(
    {{ENTITY}}Handler {{entity}}Handler,
    IHttpContextAccessor httpContextAccessor
) : I{{ENTITY}}Controller
{
    public async Task<ActionResult<Get{{ENTITY}}ResponsePaginatedList>> Get{{ENTITY}}OffsetAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var request = httpContextAccessor.HttpContext.Request;
        var pageSize = 10;
        var pageIndex = 0;
        var noCache = false;

        if (request.Headers.TryGetValue("x-page-index", out var pageIndexText))
        {
            int.TryParse(pageIndexText, out pageIndex);
        }

        if (request.Headers.TryGetValue("x-page-size", out var pageSizeText))
        {
            int.TryParse(pageSizeText, out pageSize);
        }

        if (request.Headers.TryGetValue("cache-control", out var noCacheText))
        {
            bool.TryParse(noCacheText, out noCache);
        }

        var result = await {{entity}}Handler.GetOffsetAsync(pageIndex, pageSize, noCache, cancellationToken);
        return result.ToApiResult();
    }

    public async Task<ActionResult<Get{{ENTITY}}ResponseCursorPaginatedList>> Get{{ENTITY}}CursorAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var noCache = true;
        var pageSize = this.TryGetPageSize();
        var nextPageToken = this.TryGetPageToken();
        var result = await {{entity}}Handler.GetCursorAsync(pageSize, nextPageToken, noCache, cancellationToken);
        return result.ToApiResult();
    }

    public async Task<ActionResult<Get{{ENTITY}}Response>> Get{{ENTITY}}ByIdAsync(
        int id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var result = await {{entity}}Handler.GetByIdAsync(id, cancellationToken);
        
        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        // TODO: 將實體轉換為回應模型
        var response = MapToResponse(result.Value);
        return Ok(response);
    }

    public async Task<IActionResult> Create{{ENTITY}}Async(
        Contract.Create{{ENTITY}}Request body,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var request = new Create{{ENTITY}}Request 
        { 
            Name = body.Name,
            // TODO: 對應其他屬性
        };

        var result = await {{entity}}Handler.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        // 回傳建立的實體 ID 或完整物件
        var response = MapToResponse(result.Value);
        return CreatedAtAction(
            nameof(Get{{ENTITY}}ByIdAsync), 
            new { id = result.Value.Id }, 
            response);
    }

    public async Task<IActionResult> Update{{ENTITY}}Async(
        int id,
        Contract.Update{{ENTITY}}Request body,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var request = new Update{{ENTITY}}Request 
        { 
            Name = body.Name,
            // TODO: 對應其他屬性
        };

        var result = await {{entity}}Handler.UpdateAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        var response = MapToResponse(result.Value);
        return Ok(response);
    }

    public async Task<IActionResult> Delete{{ENTITY}}Async(
        int id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var result = await {{entity}}Handler.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        return NoContent();
    }

    // 可選：搜尋功能
    public async Task<ActionResult<List<Get{{ENTITY}}Response>>> Search{{ENTITY}}Async(
        string keyword,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest("搜尋關鍵字不能為空");
        }

        var result = await {{entity}}Handler.SearchAsync(keyword, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        var responses = result.Value.Select(MapToResponse).ToList();
        return Ok(responses);
    }

    private int TryGetPageSize()
    {
        var request = httpContextAccessor.HttpContext.Request;

        return request.Headers.TryGetValue("x-page-size", out var pageSize)
            ? int.Parse(pageSize.FirstOrDefault() ?? string.Empty)
            : 10;
    }

    private string TryGetPageToken()
    {
        var request = httpContextAccessor.HttpContext.Request;

        if (request.Headers.TryGetValue("x-next-page-token", out var nextPageToken))
        {
            return nextPageToken;
        }

        return null;
    }

    private Get{{ENTITY}}Response MapToResponse({{ENTITY}} entity)
    {
        // TODO: 實作實體到回應模型的對應
        return new Get{{ENTITY}}Response
        {
            Id = entity.Id,
            Name = entity.Name,
            // TODO: 對應其他屬性
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    // 可選：批量操作
    public async Task<IActionResult> BulkCreate{{ENTITY}}Async(
        List<Contract.Create{{ENTITY}}Request> requests,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (requests == null || !requests.Any())
        {
            return BadRequest("請求列表不能為空");
        }

        var results = new List<Get{{ENTITY}}Response>();
        var failures = new List<Failure>();

        foreach (var body in requests)
        {
            var request = new Create{{ENTITY}}Request 
            { 
                Name = body.Name,
                // TODO: 對應其他屬性
            };

            var result = await {{entity}}Handler.CreateAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                results.Add(MapToResponse(result.Value));
            }
            else
            {
                failures.Add(result.Error);
            }
        }

        if (failures.Any())
        {
            return BadRequest(new 
            {
                Message = "部分請求失敗",
                Successes = results.Count,
                Failures = failures
            });
        }

        return Ok(results);
    }
}