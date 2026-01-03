using System.Diagnostics;

namespace JobBank1111.Job.WebAPI;

public class MeasurementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MeasurementMiddleware> _logger;

    public MeasurementMiddleware(RequestDelegate next, ILogger<MeasurementMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = httpContext.TraceIdentifier;

        try
        {
            await _next(httpContext);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request completed | RequestId: {RequestId} | Duration: {Duration}ms",
                requestId,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}