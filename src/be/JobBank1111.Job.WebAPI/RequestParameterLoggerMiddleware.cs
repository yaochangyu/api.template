using System.Text.Json;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI;

public class RequestParameterLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestParameterLoggerMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RequestParameterLoggerMiddleware(
        RequestDelegate next,
        ILogger<RequestParameterLoggerMiddleware> logger,
        JsonSerializerOptions jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var exceptionOccurred = false;
        
        try
        {
            await _next(httpContext);
        }
        catch
        {
            // 標記有例外發生，讓 ExceptionHandlingMiddleware 處理
            exceptionOccurred = true;
            throw; // 重新拋出例外讓上層處理
        }

        // 只有在沒有例外發生時才記錄請求參數
        if (!exceptionOccurred)
        {
            await LogRequestParametersAsync(httpContext);
        }
    }

    private async Task LogRequestParametersAsync(HttpContext context)
    {
        try
        {
            var traceContext = GetTraceContext(context);
            var requestInfo = await RequestInfoExtractor.ExtractRequestInfoAsync(context, _jsonOptions);

            _logger.LogInformation(
                "Request completed successfully - {Method} {Path} | TraceId: {TraceId} | UserId: {UserId} | RequestInfo: {@RequestInfo}",
                context.Request.Method,
                context.Request.Path,
                traceContext.TraceId,
                traceContext.UserId,
                requestInfo);
        }
        catch (Exception ex)
        {
            // 記錄請求參數時發生錯誤，但不影響主要流程
            _logger.LogWarning(ex, "Failed to log request parameters");
        }
    }

    private TraceContext GetTraceContext(HttpContext context)
    {
        var contextGetter = context.RequestServices.GetService<IContextGetter<TraceContext>>();
        return contextGetter?.Get() ?? new TraceContext
        {
            TraceId = context.TraceIdentifier,
            UserId = "anonymous"
        };
    }
}
