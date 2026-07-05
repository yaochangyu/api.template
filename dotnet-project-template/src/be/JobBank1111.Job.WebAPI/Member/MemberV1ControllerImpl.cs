using JobBank1111.Job.WebAPI.Contract;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

public class MemberV1ControllerImpl(
    MemberHandler memberHandler,
    IHttpContextAccessor httpContextAccessor
) : IMemberV1Controller
{
    public async Task<ActionResult<GetMemberResponseCursorPaginatedList>> GetMemberCursorV1Async(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var noCache = true;
        var pageSize = this.TryGetPageSize();
        var nextPageToken = this.TryGetPageToken();
        var result = await memberHandler.GetMembersCursorAsync(pageSize, nextPageToken, noCache, cancellationToken);
        return result.ToActionResult();
    }

    public async Task<ActionResult<GetMemberResponsePaginatedList>> GetMemberOffsetV1Async(
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

        var result = await memberHandler.GetMemberOffsetAsync(pageIndex, pageSize, noCache, cancellationToken);
        return result.ToActionResult();
    }

    public async Task<ActionResult<Contract.InsertMemberResponse>> InsertMemberV1Async(
        Contract.InsertMemberRequest body,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var result = await memberHandler.InsertAsync(
            new InsertMemberRequest { Email = body.Email, Name = body.Name, Age = body.Age, }, cancellationToken);
        return result.ToActionResult();
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
}
