using System.Diagnostics;
using System.Text;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI;

public class LoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggerMiddleware> _logger;

    public LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopwatch = Stopwatch.StartNew();
        var traceContext = GetTraceContext(httpContext);

        // 記錄請求開始
        LogRequestStart(httpContext, traceContext);

        try
        {
            await _next(httpContext);
            stopwatch.Stop();
        }
        finally
        {
            // 記錄請求成功完成
            LogRequestCompleted(httpContext, traceContext, stopwatch.ElapsedMilliseconds);
        }
    }

    private TraceContext GetTraceContext(HttpContext httpContext)
    {
        var contextGetter = httpContext.RequestServices.GetService<IContextGetter<TraceContext>>();
        return contextGetter?.Get() ?? new TraceContext
        {
            TraceId = httpContext.TraceIdentifier, UserId = "anonymous"
        };
    }

    private void LogRequestStart(HttpContext httpContext, TraceContext traceContext)
    {
        _logger.LogInformation(
            "Request started - {Method} {Path} | TraceId: {TraceId} | UserId: {UserId} | RemoteIP: {RemoteIP}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceContext.TraceId,
            traceContext.UserId,
            GetClientIpAddress(httpContext)
        );
    }

    private void LogRequestCompleted(HttpContext httpContext, TraceContext traceContext, long elapsedMilliseconds)
    {
        var logLevel = httpContext.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(logLevel,
            "Request completed - {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | TraceId: {TraceId} | UserId: {UserId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Response.StatusCode,
            elapsedMilliseconds,
            traceContext.TraceId,
            traceContext.UserId
        );
    }


    private static string GetClientIpAddress(HttpContext httpContext)
    {
        return httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
               ?? httpContext.Connection.RemoteIpAddress?.ToString()
               ?? "unknown";
    }
}
