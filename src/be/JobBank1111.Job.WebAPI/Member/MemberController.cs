using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

[ApiController]
public class MemberController(
    MemberHandler memberHandler) : ControllerBase
{
    [HttpGet]
    [Route("api/v1/members:cursor")]
    public async Task<ActionResult> GetCursor(
        CancellationToken cancel = default)
    {
        var noCache = true;
        var pageSize = this.TryGetPageSize();
        var nextPageToken = this.TryGetPageToken();
        var result = await memberHandler.GetMembersAsync(pageSize, nextPageToken, noCache, cancel);
        return this.Ok(result);
    }

    [HttpGet]
    [Route("api/v1/members:offset", Name = "GetAllMembers")]
    public async Task<ActionResult> GetOffset(
        CancellationToken cancel = default)
    {
        var pageSize = 10;
        var pageIndex = 0;
        var noCache = true;
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

        var result = await memberHandler.GetMembersAsync(pageIndex, pageSize, noCache, cancel);
        return this.Ok(result);
    }

    [HttpPost]
    [Route("api/v2/members", Name = "InsertMember2")]
    public async Task<ActionResult> InsertMember2Async(InsertMemberRequest request,
                                                       CancellationToken cancel = default)
    {
        return this.NoContent();
    }

    [HttpPost]
    [Route("api/v1/members", Name = "InsertMember1")]
    public async Task<ActionResult> InsertMemberAsync(InsertMemberRequest request,
                                                      CancellationToken cancel = default)
    {
        var result = await memberHandler.InsertAsync(request, cancel);
        if (result.IsFailure)
        {
            if (result.TryGetError(out var failure))
            {
                return this.BadRequest(failure);
            }
        }

        return this.Ok(result.Value);
    }

    private int TryGetPageSize()
    {
        return this.Request.Headers.TryGetValue("x-page-size", out var pageSize)
            ? int.Parse(pageSize.FirstOrDefault() ?? string.Empty)
            : 10;
    }

    private string TryGetPageToken()
    {
        if (this.Request.Headers.TryGetValue("x-next-page-token", out var nextPageToken))
        {
            return nextPageToken;
        }

        return null;
    }
}