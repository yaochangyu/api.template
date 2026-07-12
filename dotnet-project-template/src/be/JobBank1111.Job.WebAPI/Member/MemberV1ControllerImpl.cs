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
        var request = httpContextAccessor.HttpContext.Request;
        var noCache = true;
        var pageSize = request.GetPageSize();
        var nextPageToken = request.GetNextPageToken();
        var result = await memberHandler.GetMembersCursorAsync(pageSize, nextPageToken, noCache, cancellationToken);
        return result.ToActionResult();
    }

    public async Task<ActionResult<GetMemberResponsePaginatedList>> GetMemberOffsetV1Async(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var request = httpContextAccessor.HttpContext.Request;
        var pageIndex = request.GetPageIndex();
        var pageSize = request.GetPageSize();
        var noCache = request.GetNoCache();
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
}
