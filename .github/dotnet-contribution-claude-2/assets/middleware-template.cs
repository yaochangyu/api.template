using Microsoft.AspNetCore.Http;

namespace JobBank1111.Job.WebAPI;

/// <summary>
/// 自訂中介軟體範本
/// 職責：專注於單一關注點
/// </summary>
public class {MiddlewareName}Middleware(
    RequestDelegate next,
    ILogger<{MiddlewareName}Middleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // ========================================
        // 前置處理（Before）
        // ========================================
        try
        {
            // 在這裡執行前置邏輯
            // 例如：記錄請求開始時間、設定上下文等

            logger.LogInformation(
                "{MiddlewareName} - 請求開始: {Method} {Path}",
                nameof({MiddlewareName}Middleware),
                context.Request.Method,
                context.Request.Path);

            // ========================================
            // 呼叫下一個中介軟體
            // ========================================
            await next(context);

            // ========================================
            // 後置處理（After）
            // ========================================
            // 在這裡執行後置邏輯
            // 例如：記錄請求完成時間、清理資源等

            logger.LogInformation(
                "{MiddlewareName} - 請求完成: {StatusCode}",
                nameof({MiddlewareName}Middleware),
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            // ========================================
            // 例外處理（如果需要）
            // ========================================
            // ⚠️ 注意：一般建議在 ExceptionHandlingMiddleware 集中處理例外
            // 只有在特定需求時才在此處理

            logger.LogError(ex,
                "{MiddlewareName} - 發生錯誤",
                nameof({MiddlewareName}Middleware));

            throw;  // 重新拋出，讓 ExceptionHandlingMiddleware 處理
        }
    }
}

// ========================================
// 中介軟體擴充方法
// ========================================

public static class {MiddlewareName}MiddlewareExtensions
{
    public static IApplicationBuilder Use{MiddlewareName}(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<{MiddlewareName}Middleware>();
    }
}

// ========================================
// 範例：簡單的日誌中介軟體
// ========================================

/// <summary>
/// 請求日誌中介軟體
/// </summary>
public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();

        logger.LogInformation(
            "請求完成 - {Method} {Path} {StatusCode} - 耗時 {ElapsedMs} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}

// ========================================
// 範例：自訂標頭中介軟體
// ========================================

/// <summary>
/// 自訂標頭中介軟體
/// </summary>
public class CustomHeaderMiddleware(
    RequestDelegate next,
    ILogger<CustomHeaderMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 讀取自訂標頭
        var customValue = context.Request.Headers["X-Custom-Header"].FirstOrDefault();

        if (!string.IsNullOrEmpty(customValue))
        {
            logger.LogInformation("收到自訂標頭: {CustomValue}", customValue);
        }

        await next(context);

        // 設定回應標頭
        context.Response.Headers["X-Custom-Response"] = "CustomValue";
    }
}

// ========================================
// 範例：短路中介軟體（不呼叫 next）
// ========================================

/// <summary>
/// 健康檢查中介軟體（範例）
/// </summary>
public class HealthCheckMiddleware(
    RequestDelegate next,
    ILogger<HealthCheckMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 如果是健康檢查端點，直接回應，不呼叫 next
        if (context.Request.Path == "/health")
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow
            });

            logger.LogDebug("健康檢查請求");

            return;  // ⚠️ 短路，不呼叫 next
        }

        // 其他請求繼續執行
        await next(context);
    }
}
