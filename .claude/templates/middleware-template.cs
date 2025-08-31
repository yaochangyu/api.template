using System.Text.Json;
using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI;

public class {{MIDDLEWARE_NAME}}Middleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<{{MIDDLEWARE_NAME}}Middleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public {{MIDDLEWARE_NAME}}Middleware(
        RequestDelegate next,
        ILogger<{{MIDDLEWARE_NAME}}Middleware> logger,
        JsonSerializerOptions jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceContext = GetTraceContext(context);
        
        // 記錄中介軟體開始處理
        _logger.LogInformation("{{MIDDLEWARE_NAME}}Middleware 開始處理請求 - TraceId: {TraceId}", 
            traceContext?.TraceId);

        try
        {
            // 前置處理邏輯
            await PreProcessAsync(context, traceContext);

            // 繼續執行管線
            await _next(context);

            // 後置處理邏輯
            await PostProcessAsync(context, traceContext);

            _logger.LogInformation("{{MIDDLEWARE_NAME}}Middleware 處理完成 - TraceId: {TraceId}", 
                traceContext?.TraceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{{MIDDLEWARE_NAME}}Middleware 處理時發生錯誤 - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            // 依據中介軟體性質決定是否重新拋出例外
            // 如果是關鍵中介軟體，應該重新拋出讓 ExceptionHandlingMiddleware 處理
            throw;
        }
    }

    private async Task PreProcessAsync(HttpContext context, TraceContext traceContext)
    {
        // TODO: 實作前置處理邏輯
        // 例如：驗證、授權、速率限制、請求轉換等

        // 範例：驗證特定標頭
        if (!context.Request.Headers.TryGetValue("X-Custom-Header", out var headerValue))
        {
            _logger.LogWarning("缺少必要標頭 X-Custom-Header - TraceId: {TraceId}", 
                traceContext?.TraceId);
            
            // 可選：直接回應錯誤或繼續處理
            // context.Response.StatusCode = 400;
            // await context.Response.WriteAsJsonAsync(new Failure
            // {
            //     Code = nameof(FailureCode.ValidationError),
            //     Message = "缺少必要標頭",
            //     TraceId = traceContext?.TraceId
            // });
            // return;
        }

        // 範例：請求大小限制
        var contentLength = context.Request.ContentLength;
        if (contentLength > 10 * 1024 * 1024) // 10MB
        {
            _logger.LogWarning("請求大小超過限制: {ContentLength} bytes - TraceId: {TraceId}", 
                contentLength, traceContext?.TraceId);
            
            context.Response.StatusCode = 413; // Payload Too Large
            await context.Response.WriteAsJsonAsync(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "請求內容過大",
                TraceId = traceContext?.TraceId
            });
            return;
        }

        // 記錄處理開始時間
        context.Items["{{MIDDLEWARE_NAME}}_StartTime"] = DateTime.UtcNow;
    }

    private async Task PostProcessAsync(HttpContext context, TraceContext traceContext)
    {
        // TODO: 實作後置處理邏輯
        // 例如：回應轉換、標頭設定、效能記錄等

        // 範例：計算處理時間
        if (context.Items["{{MIDDLEWARE_NAME}}_StartTime"] is DateTime startTime)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("{{MIDDLEWARE_NAME}} 處理時間: {Duration}ms - TraceId: {TraceId}", 
                duration.TotalMilliseconds, traceContext?.TraceId);

            // 可選：加入回應標頭
            context.Response.Headers.TryAdd("X-Processing-Time-Ms", 
                duration.TotalMilliseconds.ToString("F2"));
        }

        // 範例：加入安全標頭
        context.Response.Headers.TryAdd("X-{{MIDDLEWARE_NAME}}-Version", "1.0");
        
        // 範例：記錄回應狀態
        _logger.LogInformation("回應狀態碼: {StatusCode} - TraceId: {TraceId}", 
            context.Response.StatusCode, traceContext?.TraceId);
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

    // 可選：輔助方法

    private async Task<string> ReadRequestBodyAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0; // 重置位置供後續中介軟體使用
        return body;
    }

    private bool IsHealthCheckRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/health") ||
               context.Request.Path.StartsWithSegments("/healthz");
    }

    private bool ShouldSkipProcessing(HttpContext context)
    {
        // 可以跳過特定路徑或條件
        return IsHealthCheckRequest(context) ||
               context.Request.Path.StartsWithSegments("/swagger");
    }
}

// 擴充方法用於 Program.cs 註冊
public static class {{MIDDLEWARE_NAME}}MiddlewareExtensions
{
    public static IApplicationBuilder Use{{MIDDLEWARE_NAME}}(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<{{MIDDLEWARE_NAME}}Middleware>();
    }
}