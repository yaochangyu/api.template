using FluentResults;
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
        var result = await this._memberCommand.InsertAsync(request, cancel);
        if (result.IsFailed)
        {
            var resultError = (Failure)result.Errors[0];
            return this.BadRequest(resultError);
        }
        return this.NoContent();
    }
}