using JobBank1111.Job.WebAPI;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

[ApiController]
public class MemberV1Controller(
    MemberHandler memberHandler) : ControllerBase
{
    [HttpGet]
    [Route("api/v1/members:cursor", Name = "GetMemberCursor")]
    public async Task<ActionResult<CursorPaginatedList<GetMemberResponse>>> GetMemberCursor(
        CancellationToken cancel = default)
    {
        var noCache = true;
        var pageSize = this.TryGetPageSize();
        var nextPageToken = this.TryGetPageToken();
        var result = await memberHandler.GetMembersCursorAsync(pageSize, nextPageToken, noCache, cancel);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        return this.Ok(result.Value);
    }

    [HttpGet]
    [Route("api/v1/members:offset", Name = "GetMemberOffset")]
    public async Task<ActionResult<PaginatedList<GetMemberResponse>>> GetMemberOffset(
        CancellationToken cancel = default)
    {
        var pageSize = 10;
        var pageIndex = 0;
        var noCache = false;

        if (this.Request.Headers.TryGetValue("x-page-index", out var pageIndexText))
        {
            int.TryParse(pageIndexText, out pageIndex);
        }

        if (this.Request.Headers.TryGetValue("x-page-size", out var pageSizeText))
        {
            int.TryParse(pageSizeText, out pageSize);
        }

        if (this.Request.Headers.TryGetValue("cache-control", out var noCacheText))
        {
            bool.TryParse(noCacheText, out noCache);
        }

        var result = await memberHandler.GetMemberOffsetAsync(pageIndex, pageSize, noCache, cancel);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        return this.Ok(result.Value);
    }

    [HttpPost]
    [Route("api/v1/members", Name = "InsertMember1")]
    public async Task<ActionResult<Member>> InsertMemberAsync(
        [FromBody] InsertMemberRequest request,
        CancellationToken cancel = default)
    {
        var result = await memberHandler.InsertAsync(request, cancel);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        return this.Ok(result.Value);
    }

    private int TryGetPageSize()
    {
        return this.Request.Headers.TryGetValue("x-page-size", out var pageSize)
            ? int.Parse(pageSize.FirstOrDefault() ?? string.Empty)
            : 10;
    }

    private string? TryGetPageToken()
    {
        if (this.Request.Headers.TryGetValue("x-next-page-token", out var nextPageToken))
        {
            return nextPageToken;
        }

        return null;
    }
}
