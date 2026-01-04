using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI;

[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    [Route("api/v1/tests")]
    public async Task<ActionResult> GetTests(
        CancellationToken cancel = default)
    {
        return this.Ok();
    }
}