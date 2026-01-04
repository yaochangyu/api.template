using JobBank1111.Job.WebAPI.Contract;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

public class MemberControllerImpl(
    MemberHandler memberHandler,
    IHttpContextAccessor httpContextAccessor
) : IMemberController
{
    public async Task<ActionResult<GetMemberResponseCursorPaginatedList>> GetMembersCursorAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var noCache = true;
        var pageSize = this.TryGetPageSize();
        var nextPageToken = this.TryGetPageToken();
        var result = await memberHandler.GetMembersCursorAsync(pageSize, nextPageToken, noCache, cancellationToken);
        return result.ToActionResult();
    }

    public async Task<ActionResult<GetMemberResponsePaginatedList>> GetMemberOffsetAsync(
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

    public async Task<IActionResult> InsertMember2Async(Contract.InsertMemberRequest body,
        CancellationToken cancellationToken =
            default(CancellationToken))
    {
        return new NoContentResult();
    }

    public async Task<IActionResult> InsertMember1Async(Contract.InsertMemberRequest body,
        CancellationToken cancellationToken =
            default(CancellationToken))
    {
        var result = await memberHandler.InsertAsync(
            new InsertMemberRequest { Email = body.Email, Name = body.Name, Age = body.Age, }, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToFailureResult();
        }

        return new NoContentResult();
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