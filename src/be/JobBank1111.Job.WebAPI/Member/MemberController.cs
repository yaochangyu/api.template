using JobBank1111.Infrastructure.TraceContext;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

[ApiController]
public class MemberController(
    MemberHandler memberHandler) : ControllerBase
{
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

    [HttpPost]
    [Route("api/v2/members", Name = "InsertMember2")]
    public async Task<ActionResult> InsertMember2Async(InsertMemberRequest request,
                                                       CancellationToken cancel = default)
    {
        return this.NoContent();
    }

    [HttpGet]
    [Route("api/v1/members", Name = "GetAllMembers")]
    public async Task<ActionResult> GetAllMembersAsync(
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
}