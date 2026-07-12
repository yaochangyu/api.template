using System.Net;
using System.Text.Json;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        JsonSerializerOptions jsonOptions,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions;
        _environment = environment;
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
        
        // 擷取請求資訊用於日誌記錄
        var requestInfo = await RequestInfoExtractor.ExtractRequestInfoAsync(context, _jsonOptions);
        
        // 記錄未處理的例外，包含所有請求資訊
        _logger.LogError(exception,
            "Unhandled exception occurred - {Method} {Path} | TraceId: {TraceId} | UserId: {UserId} | ExceptionType: {ExceptionType} | RequestInfo: {@RequestInfo}",
            context.Request.Method,
            context.Request.Path,
            traceContext.TraceId,
            traceContext.UserId,
            exception.GetType().Name,
            requestInfo);

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
        // 非 Development 環境不回傳例外原文，避免連線字串、SQL 錯誤等內部細節出站；
        // TraceId 保留，足以關聯 server log（health-check 2026-07-12 D5）
        var message = _environment.IsDevelopment()
            ? exception.Message
            : "系統發生未預期的錯誤，請提供 TraceId 給系統管理員查詢";

        return new Failure
        {
            Code = nameof(FailureCode.Unknown),
            Message = message,
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