using JobBank1111.Infrastructure.TraceContext;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

[ApiController]
[Route("[controller]")]
public class MemberController(
    MemberHandler memberHandler) : ControllerBase
{
    [HttpPost(Name = "InsertMember")]
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
    
    [HttpGet(Name = "GetAllMembers")]
    [Route("/members")]
    public async Task<ActionResult> GetAllMembersAsync(
                                                      CancellationToken cancel = default)
    {
        var result = await memberHandler.GetAllMembersAsync(cancel);
        return this.Ok(result);
    }

}