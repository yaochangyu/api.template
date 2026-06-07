using Microsoft.AspNetCore.Mvc;

namespace JobBank1111.Job.WebAPI;

[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    [Route("api/v1/tests")]
    public ActionResult GetTests(
        [FromQuery] string userId,
        [FromQuery] string description)
    {
        return this.Ok(new
        {
            userId,
            description
        });
    }
}