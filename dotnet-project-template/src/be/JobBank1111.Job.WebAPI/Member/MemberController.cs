using JobBank1111.Job.WebAPI.Contract;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

[ApiController]
public class MemberController(
    MemberHandler memberHandler,
    IHttpContextAccessor httpContextAccessor
) : ControllerBase
{
    [HttpGet("api/v2/members:cursor")]
    public async Task<ActionResult<GetMemberResponseCursorPaginatedList>> GetMembersCursorAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var request = httpContextAccessor.HttpContext.Request;
        var noCache = true;
        var pageSize = request.GetPageSize();
        var nextPageToken = request.GetNextPageToken();
        var result = await memberHandler.GetMembersCursorAsync(pageSize, nextPageToken, noCache, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("api/v2/members:offset")]
    public async Task<ActionResult<GetMemberResponsePaginatedList>> GetMemberOffsetAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var request = httpContextAccessor.HttpContext.Request;
        var pageIndex = request.GetPageIndex();
        var pageSize = request.GetPageSize();
        var noCache = request.GetNoCache();
        var result = await memberHandler.GetMemberOffsetAsync(pageIndex, pageSize, noCache, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("api/v2/members")]
    public async Task<IActionResult> InsertMemberAsync(Contract.InsertMemberRequest body,
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
}