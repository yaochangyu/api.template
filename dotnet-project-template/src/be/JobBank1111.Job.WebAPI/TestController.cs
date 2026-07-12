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

    /// <summary>
    /// 示範／測試用：觸發未處理例外，供整合測試驗證
    /// ExceptionHandlingMiddleware 在非 Development 環境不回傳例外原文（D5）
    /// </summary>
    [HttpGet]
    [Route("api/v1/tests:throw")]
    public ActionResult ThrowException()
    {
        throw new InvalidOperationException("敏感內部錯誤：Server=db;Password=secret");
    }
}