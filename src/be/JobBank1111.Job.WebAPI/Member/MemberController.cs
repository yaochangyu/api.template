using JobBank1111.Infrastructure.TraceContext;
using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.Member;

[ApiController]
[Route("[controller]")]
public class MemberController(
    ILogger<MemberController> logger,
    MemberCommand memberCommand)
    : ControllerBase
{
    private MemberCommand _memberCommand = memberCommand;

    [HttpPost(Name = "InsertMember")]
    public async Task<ActionResult> InsertMember(InsertMemberRequest request,
                                                 CancellationToken cancel = default)
    {
        await this._memberCommand.InsertAsync(request, cancel);
        return this.NoContent();
    }
}