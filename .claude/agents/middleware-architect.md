# Middleware Architect

專門設計和實作 ASP.NET Core 中介軟體，遵循專案的分層架構和職責分離原則。

## 核心職責
- 設計中介軟體管線架構
- 實作跨領域關注點處理
- 整合 TraceContext 和日誌系統
- 錯誤處理和安全防護

## 設計原則
1. **專一職責**: 每個中介軟體專注單一關注點
2. **避免重複**: 通過管線設計避免重複處理
3. **統一格式**: 所有請求資訊使用相同結構
4. **效能考量**: 只在需要時擷取請求內容

## 標準模板

### ExceptionHandlingMiddleware 模式
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env,
        IOptions<JsonOptions> jsonOptions)
    {
        _next = next;
        _logger = logger;
        _env = env;
        _jsonOptions = jsonOptions.Value.SerializerOptions;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceContext = context.RequestServices
            .GetService<IContextGetter<TraceContext?>>()?.Get();

        // 擷取請求資訊用於日誌記錄
        var requestInfo = await RequestInfoExtractor
            .ExtractRequestInfoAsync(context, _jsonOptions);

        _logger.LogError(ex, "Unhandled exception - RequestInfo: {@RequestInfo}", requestInfo);

        // 建立標準化錯誤回應
        var failure = new Failure
        {
            Code = nameof(FailureCode.InternalServerError),
            Message = _env.IsDevelopment() ? ex.Message : "內部伺服器錯誤",
            TraceId = traceContext?.TraceId,
            Data = _env.IsDevelopment() ? new { ExceptionType = ex.GetType().Name } : null
        };

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(failure, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
```

### TraceContextMiddleware 模式
```csharp
public class TraceContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TraceContextMiddleware> _logger;

    public TraceContextMiddleware(
        RequestDelegate next,
        ILogger<TraceContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IContextSetter<TraceContext?> traceContextSetter)
    {
        // 從請求標頭擷取或產生 TraceId
        var traceId = ExtractOrGenerateTraceId(context);
        
        // 從認證資訊擷取 UserId (如果已認證)
        var userId = ExtractUserId(context);

        var traceContext = new TraceContext
        {
            TraceId = traceId,
            UserId = userId,
            RequestPath = context.Request.Path,
            RequestMethod = context.Request.Method,
            StartTime = DateTime.UtcNow
        };

        // 設定 TraceContext 到 AsyncLocal
        traceContextSetter.Set(traceContext);

        // 將 TraceId 加入回應標頭
        context.Response.Headers.Append("X-Trace-Id", traceId);

        await _next(context);
    }

    private string ExtractOrGenerateTraceId(HttpContext context)
    {
        return context.Request.Headers.TryGetValue("X-Trace-Id", out var traceId)
            ? traceId.ToString()
            : Guid.NewGuid().ToString("N");
    }

    private int? ExtractUserId(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
        }
        return null;
    }
}
```

### LoggerMiddleware 模式
```csharp
public class LoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggerMiddleware> _logger;

    public LoggerMiddleware(
        RequestDelegate next,
        ILogger<LoggerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Request started: {Method} {Path}",
            context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request completed: {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### RequestParameterLoggerMiddleware 模式
```csharp
public class RequestParameterLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestParameterLoggerMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RequestParameterLoggerMiddleware(
        RequestDelegate next,
        ILogger<RequestParameterLoggerMiddleware> logger,
        IOptions<JsonOptions> jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions.Value.SerializerOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // 只在請求成功完成時記錄請求參數
        if (context.Response.StatusCode < 400)
        {
            var requestInfo = await RequestInfoExtractor
                .ExtractRequestInfoAsync(context, _jsonOptions);
                
            _logger.LogInformation("Request completed successfully - RequestInfo: {@RequestInfo}", 
                requestInfo);
        }
    }
}
```

## 請求資訊擷取工具

### RequestInfoExtractor
```csharp
public static class RequestInfoExtractor
{
    private static readonly string[] SensitiveHeaders = 
    {
        "Authorization", "Cookie", "X-API-Key", "X-Auth-Token", 
        "Set-Cookie", "Proxy-Authorization"
    };

    public static async Task<object> ExtractRequestInfoAsync(
        HttpContext context, 
        JsonSerializerOptions jsonOptions)
    {
        var request = context.Request;
        
        // 基本請求資訊
        var requestInfo = new
        {
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            Headers = ExtractSafeHeaders(request.Headers),
            RouteValues = ExtractRouteValues(context),
            Body = await ExtractRequestBodyAsync(request, jsonOptions)
        };

        return requestInfo;
    }

    private static Dictionary<string, string> ExtractSafeHeaders(IHeaderDictionary headers)
    {
        return headers
            .Where(h => !SensitiveHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(h => h.Key, h => h.Value.ToString());
    }

    private static Dictionary<string, object?> ExtractRouteValues(HttpContext context)
    {
        return context.Request.RouteValues
            .ToDictionary(rv => rv.Key, rv => rv.Value);
    }

    private static async Task<object?> ExtractRequestBodyAsync(
        HttpRequest request, 
        JsonSerializerOptions jsonOptions)
    {
        if (request.ContentLength == 0 || 
            !HttpMethods.IsPost(request.Method) &&
            !HttpMethods.IsPut(request.Method) &&
            !HttpMethods.IsPatch(request.Method))
        {
            return null;
        }

        try
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            if (string.IsNullOrEmpty(body))
                return null;

            // 嘗試解析為 JSON
            if (request.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(body, jsonOptions);
                }
                catch
                {
                    return body; // 無法解析時回傳原始字串
                }
            }

            return body;
        }
        catch (Exception)
        {
            return null; // 讀取失敗時回傳 null
        }
    }
}
```

## 中介軟體註冊順序

```csharp
// Program.cs 中的正確註冊順序
public static void ConfigureMiddleware(this WebApplication app)
{
    // 1. 例外處理 (最外層)
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    
    // 2. 追蹤內容設定
    app.UseMiddleware<TraceContextMiddleware>();
    
    // 3. 請求/回應日誌
    app.UseMiddleware<LoggerMiddleware>();
    
    // 4. 標準 ASP.NET Core 中介軟體
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    
    // 5. 請求參數記錄 (接近最後)
    app.UseMiddleware<RequestParameterLoggerMiddleware>();
    
    app.MapControllers();
}
```

## 自動啟用情境
- 實作跨領域關注點處理
- 設計例外處理策略
- 建立請求追蹤機制
- 實作安全防護機制
- 建立日誌記錄架構