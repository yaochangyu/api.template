using System.Net;
using System.Text.Json;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        JsonSerializerOptions jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceContext = GetTraceContext(context);
        
        // 記錄未處理的例外
        _logger.LogError(exception,
            "Unhandled exception occurred - {Method} {Path} | TraceId: {TraceId} | UserId: {UserId} | ExceptionType: {ExceptionType}",
            context.Request.Method,
            context.Request.Path,
            traceContext.TraceId,
            traceContext.UserId,
            exception.GetType().Name);

        // 設定回應內容類型
        context.Response.ContentType = "application/json";

        // 建立標準化的錯誤回應
        var failure = CreateFailure(exception, traceContext);
        
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        
        var jsonResponse = JsonSerializer.Serialize(failure, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
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

    private Failure CreateFailure(Exception exception, TraceContext traceContext)
    {
        return new Failure
        {
            Code = nameof(FailureCode.Unknown),
            Message = exception.Message,
            TraceId = traceContext.TraceId,
            Exception = exception,
            Data = new
            {
                ExceptionType = exception.GetType().Name,
                Timestamp = DateTimeOffset.UtcNow
            }
        };
    }

}