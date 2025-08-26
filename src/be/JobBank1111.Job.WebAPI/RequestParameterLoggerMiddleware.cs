namespace JobBank1111.Job.WebAPI;

public class RequestParameterLoggerMiddleware
{
    private readonly RequestDelegate _next;

    public RequestParameterLoggerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        await _next(httpContext);
    }
}
