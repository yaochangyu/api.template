# 中介軟體架構與設計

## 管線設計原則

ASP.NET Core 中介軟體形成一條「請求-回應管線」，順序至關重要。

### 管線順序規範

```csharp
app.UseMiddleware<MeasurementMiddleware>();          // ① 最外層：計時整個請求
app.UseMiddleware<ExceptionHandlingMiddleware>();    // ② 捕捉系統異常
app.UseMiddleware<TraceContextMiddleware>();         // ③ 設定追蹤與身分
app.UseMiddleware<RequestParameterLoggerMiddleware>(); // ④ 最內層：記錄請求
```

**為什麼這個順序？**

```
客戶端請求進入
    ↓
① MeasurementMiddleware   ← 開始計時，記錄進入時間
    ↓
② ExceptionHandlingMiddleware  ← 如果後續有未捕捉異常，我捕捉
    ↓
③ TraceContextMiddleware    ← 設定 TraceId、身分、DI 容器
    ↓
④ RequestParameterLoggerMiddleware  ← 記錄本次請求的參數
    ↓
Controller/Business Logic
    ↓
(回應時反向經過)
    ↓
① MeasurementMiddleware   ← 記錄耗時、添加 response header
    ↓
客戶端收到回應
```

## 核心中介軟體職責

### 1️⃣ MeasurementMiddleware（計時與度量）

**職責**：
- 記錄請求進入時間
- 計算請求耗時
- 添加 `X-Response-Time` header（供性能監控）
- 記錄度量指標（用於 Prometheus 等監控系統）

**實作要點**：
```csharp
public class MeasurementMiddleware(ILogger<MeasurementMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            context.Response.Headers.Add("X-Response-Time", 
                stopwatch.ElapsedMilliseconds.ToString());
            
            logger.LogInformation(
                "Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);
            
            return Task.CompletedTask;
        });
        
        await next(context);
    }
}
```

### 2️⃣ ExceptionHandlingMiddleware（全局異常捕捉）

**職責**：
- 捕捉業務邏輯層未處理的異常
- 轉換為標準化 Failure 物件
- 記錄詳細的異常日誌
- 回傳 500 Internal Server Error

**重要原則**：
- Repository/Handler 使用 Result Pattern，不拋異常
- 只捕捉意外異常（非業務邏輯異常）
- 所有捕捉的異常都要記錄（用於事後根因分析）

**實作要點**：
```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
        {
            context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
            // 取消的請求通常不記錄
        }
        catch (Exception ex)
        {
            // 記錄詳細異常資訊
            logger.LogError(ex,
                "Unhandled exception in request {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var failure = new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = "An unexpected error occurred",
                TraceId = context.TraceIdentifier,
                Exception = ex  // 不序列化到客戶端
            };
            
            await context.Response.WriteAsJsonAsync(
                mapper.Map(failure).ErrorResponse);
        }
    }
}
```

### 3️⃣ TraceContextMiddleware（追蹤與身分）

**職責**：
- 提取或生成 TraceId（用於追蹤請求）
- 處理身分驗證（提取 token、設定 ClaimsPrincipal）
- 建立 TraceContext 不可變物件
- 注入到 DI 容器供後續層使用

**實作要點**：
```csharp
public class TraceContextMiddleware(
    IContextSetter<TraceContext> contextSetter,
    ILogger<TraceContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. 提取或生成 TraceId
        var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault()
            ?? context.TraceIdentifier
            ?? Guid.NewGuid().ToString();
        
        // 2. 提取身分（從 Authorization header 或 Cookie）
        var userId = ExtractUserId(context);
        
        // 3. 建立不可變 TraceContext
        var traceContext = new TraceContext
        {
            TraceId = traceId,
            UserId = userId,
            RequestTime = DateTime.UtcNow
        };
        
        // 4. 注入到 DI 容器供後續層使用
        contextSetter.Set(traceContext);
        
        // 5. 添加到回應標頭（方便客戶端追蹤）
        context.Response.Headers.Add("X-Trace-Id", traceId);
        
        await next(context);
    }
}
```

### 4️⃣ RequestParameterLoggerMiddleware（請求參數日誌）

**職責**：
- 僅在成功完成時記錄（避免錯誤請求的日誌噪音）
- 記錄路由參數、查詢參數、請求標頭、請求本文
- 自動排除敏感標頭（Authorization、Cookies 等）
- 使用結構化日誌便於分析

**實作要點**：
```csharp
public class RequestParameterLoggerMiddleware
{
    private static readonly HashSet<string> SensitiveHeaders = new()
    {
        "Authorization", "Cookie", "X-API-Key", "X-Auth-Token"
    };
    
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        
        // 只在成功時記錄（StatusCode < 400）
        if (context.Response.StatusCode >= 400)
            return;
        
        var requestInfo = new
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            Headers = ExtractSafeHeaders(context.Request.Headers),
            RouteValues = context.Request.RouteValues,
            Body = await ExtractRequestBody(context.Request)
        };
        
        logger.LogInformation(
            "Request completed successfully: {@RequestInfo}",
            requestInfo);
    }
}
```

## 職責分離檢查清單

| 層級 | 應做 ✅ | 不應做 ❌ |
|------|---------|----------|
| **MeasurementMiddleware** | 計時、添加標頭 | 異常處理、身分驗證 |
| **ExceptionHandlingMiddleware** | 捕捉異常、記錄、回傳 500 | 業務邏輯、身分驗證 |
| **TraceContextMiddleware** | 提取 TraceId、身分、DI 注入 | 日誌記錄、業務邏輯 |
| **RequestParameterLoggerMiddleware** | 記錄請求參數 | 異常處理、身分驗證 |
| **Repository/Handler** | 業務邏輯、Result Pattern | 日誌記錄、異常拋出 |

## 不應該做的事

### ❌ 在 Repository/Handler 中記錄日誌
日誌集中在 Middleware 中管理，避免重複和混亂。

### ❌ 在多個 Middleware 中記錄同一類信息
例如：不要在 ExceptionHandlingMiddleware 和 RequestParameterLoggerMiddleware 都記錄請求參數。

### ❌ 在 Middleware 中實作業務邏輯
Middleware 應只負責跨領域關注點（追蹤、日誌、異常等）。

### ❌ 忘記在 Middleware 中添加 TraceId
所有重要操作都應包含 TraceId，便於問題追蹤。
