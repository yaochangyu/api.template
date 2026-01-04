using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI.IntegrationTest._01_Demo;

[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    [Route("api/v1/tests")]
    public async Task<ActionResult> GetTests([FromQuery] string description,
                                             [FromQuery] string userId,
                                             CancellationToken cancel = default)
    {
        return this.Ok(new { description, userId });
    }
}