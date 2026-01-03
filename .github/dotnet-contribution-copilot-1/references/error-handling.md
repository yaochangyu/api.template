# 錯誤處理與回應管理

> 本專案使用 Result Pattern 處理錯誤，並透過中介軟體統一管理錯誤回應

## Result Pattern 設計

### 核心概念

**Result Pattern** 是函數式程式設計中的錯誤處理模式：
- 明確表達操作可能失敗
- 避免使用例外處理業務邏輯錯誤
- 強制呼叫者處理錯誤情況

### 使用的套件

```xml
<PackageReference Include="CSharpFunctionalExtensions" Version="3.1.0" />
```

### Result<TSuccess, TFailure> 類型

```csharp
// 成功的情況
Result<Member, Failure> result = Result.Success<Member, Failure>(member);

// 失敗的情況
Result<Member, Failure> result = Result.Failure<Member, Failure>(failure);

// 檢查結果
if (result.IsSuccess)
{
    var member = result.Value;  // 取得成功值
}
else
{
    var failure = result.Error;  // 取得錯誤資訊
}
```

## Failure 物件結構

### 標準 Failure 定義

```csharp
public record Failure
{
    /// <summary>
    /// 錯誤代碼（使用 FailureCode 列舉）
    /// </summary>
    public string Code { get; init; }
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string Message { get; init; }
    
    /// <summary>
    /// 追蹤 ID（用於日誌關聯）
    /// </summary>
    public string TraceId { get; init; }
    
    /// <summary>
    /// 原始例外物件（不序列化到客戶端）
    /// </summary>
    [JsonIgnore]
    public Exception Exception { get; init; }
    
    /// <summary>
    /// 結構化資料（額外資訊）
    /// </summary>
    public Dictionary<string, object> Data { get; init; }
}
```

### 實作參考

**位置**：`src/be/JobBank1111.Job.WebAPI/Failure.cs`

## FailureCode 列舉

### 標準錯誤代碼

```csharp
public enum FailureCode
{
    Unauthorized,        // 401 - 未授權存取
    Forbidden,           // 403 - 禁止存取
    NotFound,            // 404 - 資源不存在
    DuplicateEmail,      // 409 - 重複郵件地址
    ValidationError,     // 400 - 驗證錯誤
    InvalidOperation,    // 400 - 無效操作
    DbError,             // 500 - 資料庫錯誤
    DbConcurrency,       // 409 - 資料庫併發衝突
    Timeout,             // 408 - 逾時
    InternalServerError, // 500 - 內部伺服器錯誤
    Unknown              // 500 - 未知錯誤
}
```

### FailureCode 到 HTTP 狀態碼的映射

```csharp
public static class FailureCodeMapper
{
    private static readonly Dictionary<string, int> CodeToStatusCode = new()
    {
        [nameof(FailureCode.Unauthorized)] = 401,
        [nameof(FailureCode.Forbidden)] = 403,
        [nameof(FailureCode.NotFound)] = 404,
        [nameof(FailureCode.ValidationError)] = 400,
        [nameof(FailureCode.InvalidOperation)] = 400,
        [nameof(FailureCode.DuplicateEmail)] = 409,
        [nameof(FailureCode.DbConcurrency)] = 409,
        [nameof(FailureCode.Timeout)] = 408,
        [nameof(FailureCode.DbError)] = 500,
        [nameof(FailureCode.InternalServerError)] = 500,
        [nameof(FailureCode.Unknown)] = 500
    };
    
    public static int ToHttpStatusCode(string code)
    {
        return CodeToStatusCode.TryGetValue(code, out var statusCode)
            ? statusCode
            : 500;  // 預設為 500
    }
}
```

**實作參考**：
- `src/be/JobBank1111.Job.WebAPI/FailureCode.cs`
- `src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs`

## 分層錯誤處理策略

### Handler 層：業務邏輯錯誤

**職責**：處理預期的業務邏輯錯誤

```csharp
public class MemberHandler(MemberRepository memberRepo)
{
    public async Task<Result<Member>> GetByEmailAsync(
        string email,
        CancellationToken cancel)
    {
        // 驗證輸入
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.ValidationError),
                Message = "Email cannot be empty"
            });
        }
        
        // 呼叫 Repository
        var result = await memberRepo.GetByEmailAsync(email, cancel);
        
        // 處理結果
        if (result.IsFailure)
        {
            return Result.Failure<Member, Failure>(result.Error);
        }
        
        // 業務邏輯檢查
        if (result.Value == null)
        {
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.NotFound),
                Message = $"Member with email {email} not found"
            });
        }
        
        return Result.Success<Member, Failure>(result.Value);
    }
}
```

**重點**：
- ✅ 回傳 Result<T, Failure>
- ✅ 不拋出業務邏輯例外
- ✅ 不記錄錯誤日誌（交給 Middleware）
- ✅ 使用 nameof(FailureCode.*)

### Repository 層：資料存取錯誤

**職責**：處理資料庫操作錯誤

```csharp
public class MemberRepository(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<Result<Member>> GetByEmailAsync(
        string email,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
            
            var member = await dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Email == email, cancel);
            
            return Result.Success<Member, Failure>(member);
        }
        catch (Exception ex)
        {
            // 捕捉資料庫例外，轉換為 Failure
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Database error occurred",
                Exception = ex  // 保存原始例外
            });
        }
    }
    
    public async Task<Result<Member>> CreateAsync(
        Member member,
        CancellationToken cancel = default)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
            
            dbContext.Members.Add(member);
            await dbContext.SaveChangesAsync(cancel);
            
            return Result.Success<Member, Failure>(member);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            // 處理重複鍵例外
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.DuplicateEmail),
                Message = "Email already exists",
                Exception = ex
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // 處理併發例外
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbConcurrency),
                Message = "Concurrency conflict occurred",
                Exception = ex
            });
        }
        catch (Exception ex)
        {
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Database error occurred",
                Exception = ex
            });
        }
    }
    
    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        return ex.InnerException?.Message.Contains("duplicate key") == true ||
               ex.InnerException?.Message.Contains("UNIQUE constraint") == true;
    }
}
```

**重點**：
- ✅ 捕捉資料庫例外
- ✅ 將例外轉換為 Failure
- ✅ 保存原始例外到 Failure.Exception
- ✅ 使用明確的錯誤代碼

### Controller 層：HTTP 回應轉換

**職責**：將 Result 轉換為 HTTP 回應

```csharp
[ApiController]
[Route("api/[controller]")]
public class MembersController(
    MemberHandler memberHandler,
    IContextGetter<TraceContext> traceContextGetter) : ControllerBase
{
    [HttpGet("{email}")]
    public async Task<IActionResult> GetByEmail(
        string email,
        CancellationToken cancel)
    {
        var result = await memberHandler.GetByEmailAsync(email, cancel);
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        // 將 Failure 轉換為 HTTP 回應
        var failure = result.Error;
        var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);
        
        // 加入 TraceId
        var traceContext = traceContextGetter.Get();
        failure = failure with { TraceId = traceContext.TraceId };
        
        return StatusCode(statusCode, failure);
    }
}
```

**重點**：
- ✅ 檢查 Result.IsSuccess
- ✅ 使用 FailureCodeMapper 轉換狀態碼
- ✅ 加入 TraceId
- ✅ 不記錄錯誤日誌（交給 Middleware）

### Middleware 層：系統例外處理

**職責**：捕捉未處理的系統層級例外

```csharp
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // 記錄系統例外（結構化日誌）
            logger.LogError(ex, 
                "Unhandled exception occurred. TraceId: {TraceId}", 
                context.TraceIdentifier);
            
            // 轉換為標準化回應
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var failure = new Failure
        {
            Code = nameof(FailureCode.InternalServerError),
            Message = "An internal server error occurred",
            TraceId = context.TraceIdentifier
            // 不包含 Exception（避免洩露內部資訊）
        };
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;
        
        var json = JsonSerializer.Serialize(failure);
        await context.Response.WriteAsync(json);
    }
}
```

**實作參考**：`src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs`

**重點**：
- ✅ 記錄結構化日誌
- ✅ 包含 TraceId
- ✅ 不洩露內部實作細節
- ✅ 統一回應格式

## 錯誤處理最佳實踐

### 1. 不要重複拋出例外

```csharp
// ❌ 錯誤：處理過的例外不應再次 throw
try
{
    await DoSomethingAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error occurred");
    throw;  // 避免
}

// ✅ 正確：轉換為 Result
try
{
    await DoSomethingAsync();
    return Result.Success<T, Failure>(value);
}
catch (Exception ex)
{
    return Result.Failure<T, Failure>(new Failure
    {
        Code = nameof(FailureCode.DbError),
        Message = "Operation failed",
        Exception = ex  // 保存例外，但不 throw
    });
}
```

### 2. 統一使用 nameof(FailureCode.*)

```csharp
// ❌ 錯誤：使用字串literal
Code = "NotFound"  // 容易拼錯

// ✅ 正確：使用 nameof
Code = nameof(FailureCode.NotFound)  // 編譯時檢查
```

### 3. 必須保存原始例外

```csharp
// ❌ 錯誤：丟失原始例外資訊
catch (Exception ex)
{
    return Result.Failure<T, Failure>(new Failure
    {
        Code = nameof(FailureCode.DbError),
        Message = "Error occurred"
        // 沒有保存 Exception
    });
}

// ✅ 正確：保存原始例外
catch (Exception ex)
{
    return Result.Failure<T, Failure>(new Failure
    {
        Code = nameof(FailureCode.DbError),
        Message = "Error occurred",
        Exception = ex  // 保存用於日誌
    });
}
```

### 4. 確保包含 TraceId

```csharp
// ✅ 在 Controller 或 Middleware 加入 TraceId
var traceContext = traceContextGetter.Get();
failure = failure with { TraceId = traceContext.TraceId };
```

### 5. 分離業務錯誤與系統例外

```csharp
// ✅ 業務錯誤：使用 Result Pattern
if (member == null)
{
    return Result.Failure<Member, Failure>(new Failure
    {
        Code = nameof(FailureCode.NotFound),
        Message = "Member not found"
    });
}

// ✅ 系統例外：讓 Middleware 捕捉
// 不捕捉系統層級例外（如網路錯誤、OutOfMemoryException）
```

### 6. 業務邏輯層不記錄日誌

```csharp
// ❌ 錯誤：Handler/Repository 記錄錯誤日誌
public async Task<Result<Member>> GetByIdAsync(Guid id)
{
    var result = await repo.GetByIdAsync(id);
    
    if (result.IsFailure)
    {
        logger.LogError("Failed to get member");  // 避免
        return result;
    }
}

// ✅ 正確：只回傳 Failure，由 Middleware 記錄
public async Task<Result<Member>> GetByIdAsync(Guid id)
{
    var result = await repo.GetByIdAsync(id);
    
    // 直接回傳結果，不記錄日誌
    return result;
}
```

## 檢查清單

### Handler/Repository 層
- [ ] 回傳 Result<T, Failure>
- [ ] 不拋出業務邏輯例外
- [ ] 保存原始例外到 Failure.Exception
- [ ] 使用 nameof(FailureCode.*)
- [ ] 不記錄錯誤日誌

### Controller 層
- [ ] 檢查 Result.IsSuccess
- [ ] 使用 FailureCodeMapper 轉換狀態碼
- [ ] 加入 TraceId
- [ ] 回傳標準化的 Failure 物件

### Middleware 層
- [ ] 捕捉系統層級例外
- [ ] 記錄結構化日誌（包含 TraceId）
- [ ] 統一回應格式
- [ ] 不洩露內部實作細節

---

**參考來源**：CLAUDE.md - 錯誤處理與回應管理章節  
**實作位置**：
- `src/be/JobBank1111.Job.WebAPI/Failure.cs`
- `src/be/JobBank1111.Job.WebAPI/FailureCode.cs`
- `src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs`
- `src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs`
