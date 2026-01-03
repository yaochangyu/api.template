# 中介軟體參考文件

## 中介軟體管線架構

### 管線順序（由外至內）

```
HTTP 請求
    ↓
┌─────────────────────────────────────────┐
│ 1. MeasurementMiddleware                │ ← 最外層：度量與計時
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│ 2. ExceptionHandlingMiddleware          │ ← 系統層級例外處理
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│ 3. TraceContextMiddleware                │ ← 追蹤內容與身分資訊
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│ 4. RequestParameterLoggerMiddleware      │ ← 請求參數記錄
└─────────────────────────────────────────┘
    ↓
    Controller → Handler → Repository
    ↓
HTTP 回應
```

### Program.cs 配置

```csharp
// ⚠️ 順序非常重要，不可調換
app.UseMiddleware<MeasurementMiddleware>();           // 1. 最外層
app.UseMiddleware<ExceptionHandlingMiddleware>();     // 2. 例外處理
app.UseMiddleware<TraceContextMiddleware>();          // 3. 追蹤內容
app.UseMiddleware<RequestParameterLoggerMiddleware>();// 4. 請求記錄
```

## 各中介軟體職責

### 1. MeasurementMiddleware

**職責**：度量整體請求耗時

```csharp
public class MeasurementMiddleware(RequestDelegate next, ILogger<MeasurementMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();
        logger.LogInformation(
            "請求 {Method} {Path} 完成，耗時 {ElapsedMs} ms，狀態碼 {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds,
            context.Response.StatusCode
        );
    }
}
```

**特點**：
- ✅ 最外層中介軟體
- ✅ 包覆整體請求耗時
- ✅ 記錄完整的請求度量資訊

### 2. ExceptionHandlingMiddleware

**職責**：捕捉未處理的系統層級例外，統一回應格式

```csharp
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IContextGetter<TraceContext> contextGetter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceContext = contextGetter.Context;

            // 記錄結構化日誌
            logger.LogError(ex,
                "未處理的例外發生 - TraceId: {TraceId}, Path: {Path}",
                traceContext?.TraceId ?? context.TraceIdentifier,
                context.Request.Path);

            // 設定回應
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var failure = new Failure
            {
                Code = FailureCode.InternalServerError,
                Message = "伺服器內部錯誤，請聯絡管理員",
                TraceId = traceContext?.TraceId ?? context.TraceIdentifier
            };

            await context.Response.WriteAsJsonAsync(failure);
        }
    }
}
```

**特點**：
- ✅ 僅處理「未預期」的系統例外
- ✅ 業務邏輯錯誤應由 Result Pattern 處理，不應流到此層
- ✅ 統一回應格式
- ✅ 記錄結構化日誌
- ✅ 包含 TraceId

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs](../../src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs)

### 3. TraceContextMiddleware

**職責**：設定追蹤內容與身分資訊

```csharp
public class TraceContextMiddleware(
    RequestDelegate next,
    IContextSetter<TraceContext> contextSetter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. 從請求標頭取得或產生 TraceId
        var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault()
                      ?? Guid.NewGuid().ToString();

        // 2. 從身分驗證資訊取得 UserId（如果有）
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 3. 建立 TraceContext（不可變物件）
        var traceContext = new TraceContext
        {
            TraceId = traceId,
            UserId = userId
        };

        // 4. 設定到 AsyncLocal（整個請求生命週期可用）
        contextSetter.SetContext(traceContext);

        // 5. 將 TraceId 加入回應標頭
        context.Response.Headers["X-Trace-Id"] = traceId;

        await next(context);
    }
}
```

**特點**：
- ✅ 統一處理 TraceId 與 UserId
- ✅ 使用不可變物件（`record` 類型）
- ✅ 透過 AsyncLocal 在整個請求生命週期內可用
- ✅ 自動加入回應標頭

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs](../../src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs)

### 4. RequestParameterLoggerMiddleware

**職責**：記錄請求參數（僅在成功完成時）

```csharp
public class RequestParameterLoggerMiddleware(
    RequestDelegate next,
    ILogger<RequestParameterLoggerMiddleware> logger,
    IContextGetter<TraceContext> contextGetter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        // 僅在成功時記錄（避免與 ExceptionHandlingMiddleware 重複）
        if (context.Response.StatusCode < 400)
        {
            var traceContext = contextGetter.Context;
            var requestInfo = await RequestInfoExtractor.ExtractAsync(context);

            logger.LogInformation(
                "請求成功 - TraceId: {TraceId}, {RequestInfo}",
                traceContext?.TraceId,
                requestInfo
            );
        }
    }
}
```

**特點**：
- ✅ 僅記錄成功請求（避免重複記錄）
- ✅ 使用 RequestInfoExtractor 統一擷取資訊
- ✅ 包含 TraceId

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs](../../src/be/JobBank1111.Job.WebAPI/RequestParameterLoggerMiddleware.cs)

## RequestInfoExtractor

### 功能

統一擷取請求資訊，避免重複程式碼。

```csharp
public static class RequestInfoExtractor
{
    public static async Task<RequestInfo> ExtractAsync(HttpContext context)
    {
        var requestInfo = new RequestInfo
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            ContentType = context.Request.ContentType,
            ContentLength = context.Request.ContentLength,
            RouteValues = ExtractRouteValues(context),
            Headers = ExtractHeaders(context),
            Body = await ExtractBodyAsync(context)
        };

        return requestInfo;
    }

    private static Dictionary<string, string> ExtractRouteValues(HttpContext context)
    {
        // 擷取路由參數
        return context.Request.RouteValues
            .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "");
    }

    private static Dictionary<string, string> ExtractHeaders(HttpContext context)
    {
        // 擷取標頭（排除敏感標頭）
        var excludeHeaders = new[] { "Authorization", "Cookie", "X-API-Key" };

        return context.Request.Headers
            .Where(h => !excludeHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
    }

    private static async Task<string?> ExtractBodyAsync(HttpContext context)
    {
        // 僅處理 POST/PUT/PATCH 且為 JSON 的請求
        if (context.Request.Method is "POST" or "PUT" or "PATCH"
            && context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();
            context.Request.Body.Position = 0;

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;
            return body;
        }

        return null;
    }
}
```

**功能說明**：
1. **路由參數**：擷取 URL 路由中的參數
2. **查詢參數**：擷取 URL 查詢字串參數
3. **請求標頭**：擷取 HTTP 標頭，自動排除敏感標頭（Authorization, Cookie 等）
4. **請求本文**：對於 POST/PUT/PATCH 請求，擷取請求本文內容並嘗試解析 JSON
5. **基本資訊**：記錄 HTTP 方法、路徑、內容類型、內容長度等

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/RequestInfoExtractor.cs](../../src/be/JobBank1111.Job.WebAPI/RequestInfoExtractor.cs)

## 職責分離原則

### 清晰的責任劃分

| 中介軟體 | 職責 | 記錄日誌 | 處理例外 |
|---------|------|---------|---------|
| **MeasurementMiddleware** | 度量計時 | ✅ 效能度量 | ❌ |
| **ExceptionHandlingMiddleware** | 系統例外處理 | ✅ 錯誤日誌 | ✅ 系統例外 |
| **TraceContextMiddleware** | 追蹤內容設定 | ❌ | ❌ |
| **RequestParameterLoggerMiddleware** | 請求參數記錄 | ✅ 成功請求 | ❌ |

### 避免重複處理

**原則**：
- ✅ ExceptionHandlingMiddleware 記錄「錯誤」日誌
- ✅ RequestParameterLoggerMiddleware 記錄「成功」日誌
- ✅ 兩者不重複，透過 HTTP 狀態碼區分

**範例**：
```csharp
// ExceptionHandlingMiddleware
catch (Exception ex)
{
    logger.LogError(ex, "未處理的例外");  // ✅ 錯誤日誌
}

// RequestParameterLoggerMiddleware
if (context.Response.StatusCode < 400)
{
    logger.LogInformation("請求成功");  // ✅ 成功日誌
}
```

## TraceContext 設計

### 不可變物件

```csharp
// 使用 record 定義不可變物件
public record TraceContext
{
    public required string TraceId { get; init; }
    public string? UserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

**特點**：
- ✅ 使用 `record` 類型
- ✅ 所有屬性使用 `init` 關鍵字（不可變）
- ✅ 避免在應用程式各層間傳遞可變狀態

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/TraceContext.cs](../../src/be/JobBank1111.Job.WebAPI/TraceContext.cs)

### AsyncLocal 機制

```csharp
public interface IContextGetter<T>
{
    T? Context { get; }
}

public interface IContextSetter<T>
{
    void SetContext(T context);
}

public class AsyncLocalContextAccessor<T> : IContextGetter<T>, IContextSetter<T>
{
    private static readonly AsyncLocal<T?> _asyncLocal = new();

    public T? Context => _asyncLocal.Value;

    public void SetContext(T context)
    {
        _asyncLocal.Value = context;
    }
}
```

**註冊方式**：
```csharp
// Program.cs
builder.Services.AddSingleton<AsyncLocalContextAccessor<TraceContext>>();
builder.Services.AddSingleton<IContextGetter<TraceContext>>(
    sp => sp.GetRequiredService<AsyncLocalContextAccessor<TraceContext>>());
builder.Services.AddSingleton<IContextSetter<TraceContext>>(
    sp => sp.GetRequiredService<AsyncLocalContextAccessor<TraceContext>>());
```

**使用方式**：
```csharp
// 在 Handler 或 Repository 中注入
public class MemberHandler(
    IContextGetter<TraceContext> contextGetter,
    IMemberRepository memberRepository)
{
    public async Task<Result<Member>> GetMemberAsync(Guid id)
    {
        var traceContext = contextGetter.Context;
        // 可以使用 traceContext.TraceId
    }
}
```

## 中介軟體最佳實務

### ✅ 應該做的事

1. **專一職責**
   - 每個中介軟體只專注於一個關注點
   - 不要在中介軟體中混合多種職責

2. **避免重複**
   - 透過管線設計避免重複處理和記錄
   - 使用 HTTP 狀態碼區分成功與失敗

3. **統一格式**
   - 所有請求資訊記錄使用相同的資料結構
   - 使用 RequestInfoExtractor 統一擷取

4. **效能考量**
   - 只有在需要時才擷取請求本文
   - 避免重複讀取 Stream

5. **錯誤容錯**
   - 記錄過程中發生錯誤不影響業務邏輯執行
   - 使用 try-catch 包裹日誌記錄邏輯

### ❌ 不應該做的事

1. **不要改變管線順序**
   - 管線順序非常重要，不可隨意調換

2. **不要在多個中介軟體記錄相同資訊**
   - 避免重複日誌

3. **不要在中介軟體中實作業務邏輯**
   - 中介軟體應專注於跨領域關注點

4. **不要忽略例外處理**
   - 中介軟體中的例外處理非常重要

5. **不要洩露敏感資訊**
   - 記錄請求資訊時，排除敏感標頭（Authorization, Cookie 等）

## 程式碼範本

📝 [middleware-template.cs](../assets/middleware-template.cs) - 中介軟體範本

## 參考資源

- 📚 [CLAUDE.md](../../../CLAUDE.md) - 完整專案指導文件
- 📝 [錯誤處理](./error-handling.md) - ExceptionHandlingMiddleware 與 Result Pattern
- 📝 [架構設計](./architecture.md) - 整體架構說明
