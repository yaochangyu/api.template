# 錯誤處理參考文件

## Result Pattern 設計

### 核心架構

本專案使用 **Result Pattern** 處理所有業務邏輯錯誤，避免使用例外處理業務邏輯錯誤。

```csharp
// 使用 CSharpFunctionalExtensions 3.1.0
using CSharpFunctionalExtensions;

// Repository 與 Handler 必須回傳 Result<TSuccess, TFailure>
public async Task<Result<Member, Failure>> GetMemberAsync(Guid id)
{
    // 成功情況
    return Result.Success<Member, Failure>(member);

    // 失敗情況
    return Result.Failure<Member, Failure>(new Failure { ... });
}
```

### 為什麼使用 Result Pattern？

**❌ 傳統例外處理的問題**：
```csharp
// 不推薦：使用例外處理業務邏輯錯誤
public Member GetMember(Guid id)
{
    var member = db.Members.Find(id);
    if (member == null)
        throw new NotFoundException("會員不存在");  // ❌ 業務邏輯錯誤不應拋出例外

    if (!member.IsActive)
        throw new InvalidOperationException("會員已停用");  // ❌ 業務邏輯錯誤

    return member;
}
```

**✅ Result Pattern 的優勢**：
```csharp
// 推薦：使用 Result Pattern
public async Task<Result<Member, Failure>> GetMemberAsync(Guid id, CancellationToken cancel)
{
    var member = await db.Members.FindAsync(id, cancel);
    if (member == null)
        return Result.Failure<Member, Failure>(
            new Failure { Code = FailureCode.NotFound, Message = "會員不存在" });

    if (!member.IsActive)
        return Result.Failure<Member, Failure>(
            new Failure { Code = FailureCode.InvalidOperation, Message = "會員已停用" });

    return Result.Success<Member, Failure>(member);
}
```

**優點**：
- ✅ 明確的錯誤處理流程
- ✅ 編譯時型別檢查
- ✅ 不會遺漏錯誤處理
- ✅ 效能更好（不使用例外）
- ✅ 可測試性更高

## FailureCode 列舉

### 定義

```csharp
public enum FailureCode
{
    Unauthorized,        // 401 未授權存取
    NotFound,           // 404 資源不存在
    DbError,            // 500 資料庫錯誤
    DuplicateEmail,     // 409 重複郵件地址
    DbConcurrency,      // 409 資料庫併發衝突
    ValidationError,    // 400 驗證錯誤
    InvalidOperation,   // 400 無效操作
    Timeout,           // 408 逾時
    InternalServerError,// 500 內部伺服器錯誤
    Unknown            // 500 未知錯誤
}
```

### HTTP 狀態碼映射

使用 `FailureCodeMapper` 將 `FailureCode` 映射至 HTTP 狀態碼：

```csharp
public static class FailureCodeMapper
{
    public static int ToHttpStatusCode(FailureCode code)
    {
        return code switch
        {
            FailureCode.Unauthorized => 401,
            FailureCode.NotFound => 404,
            FailureCode.DuplicateEmail => 409,
            FailureCode.DbConcurrency => 409,
            FailureCode.ValidationError => 400,
            FailureCode.InvalidOperation => 400,
            FailureCode.Timeout => 408,
            FailureCode.DbError => 500,
            FailureCode.InternalServerError => 500,
            FailureCode.Unknown => 500,
            _ => 500
        };
    }
}
```

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs](../../src/be/JobBank1111.Job.WebAPI/FailureCodeMapper.cs)

## Failure 物件結構

### 定義

```csharp
public class Failure
{
    /// <summary>
    /// 錯誤代碼（對應 FailureCode）
    /// </summary>
    public required FailureCode Code { get; init; }

    /// <summary>
    /// 錯誤訊息（給開發者看）
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 追蹤識別碼
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// 原始例外物件（不序列化到客戶端）
    /// </summary>
    [JsonIgnore]
    public Exception? Exception { get; init; }

    /// <summary>
    /// 結構化資料（可選）
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }
}
```

### 使用範例

```csharp
// ✅ 正確：包含完整資訊
return Result.Failure<Member, Failure>(new Failure
{
    Code = FailureCode.DbError,
    Message = ex.Message,
    TraceId = traceContext.TraceId,
    Exception = ex,  // ⚠️ 重要：必須保存原始例外
    Data = new Dictionary<string, object>
    {
        ["MemberId"] = memberId
    }
});

// ❌ 錯誤：遺漏原始例外
return Result.Failure<Member, Failure>(new Failure
{
    Code = FailureCode.DbError,
    Message = "資料庫錯誤",
    // ❌ 沒有保存 Exception
});
```

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/Failure.cs](../../src/be/JobBank1111.Job.WebAPI/Failure.cs)

## 分層錯誤處理策略

### 錯誤處理責任劃分

```
┌─────────────────────────────────────────────┐
│ ExceptionHandlingMiddleware                 │ ← 系統層級例外處理（500 錯誤）
├─────────────────────────────────────────────┤
│ Controller 層                                │ ← Result Pattern 轉換為 HTTP 回應
├─────────────────────────────────────────────┤
│ Handler 層                                   │ ← 業務邏輯錯誤（400, 404, 409 等）
├─────────────────────────────────────────────┤
│ Repository 層                                │ ← 資料庫錯誤（DbError）
└─────────────────────────────────────────────┘
```

### 1. Repository 層錯誤處理

**職責**：捕捉資料庫相關錯誤，回傳 `Result<T, Failure>`

```csharp
public async Task<Result<Member, Failure>> CreateAsync(
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
    catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
    {
        // 重複鍵錯誤
        return Result.Failure<Member, Failure>(new Failure
        {
            Code = FailureCode.DuplicateEmail,
            Message = "Email 已被使用",
            TraceId = traceContext.TraceId,
            Exception = ex  // ⚠️ 必須保存原始例外
        });
    }
    catch (DbUpdateConcurrencyException ex)
    {
        // 併發衝突
        return Result.Failure<Member, Failure>(new Failure
        {
            Code = FailureCode.DbConcurrency,
            Message = "資料已被其他使用者修改",
            TraceId = traceContext.TraceId,
            Exception = ex  // ⚠️ 必須保存原始例外
        });
    }
    catch (Exception ex)
    {
        // 其他資料庫錯誤
        return Result.Failure<Member, Failure>(new Failure
        {
            Code = FailureCode.DbError,
            Message = ex.Message,
            TraceId = traceContext.TraceId,
            Exception = ex  // ⚠️ 必須保存原始例外
        });
    }
}
```

### 2. Handler 層錯誤處理

**職責**：處理業務邏輯錯誤，協調 Repository 回傳的 Result

```csharp
public async Task<Result<MemberResponse, Failure>> CreateMemberAsync(
    CreateMemberRequest request,
    CancellationToken cancel = default)
{
    // 業務邏輯驗證
    if (string.IsNullOrWhiteSpace(request.Email))
    {
        return Result.Failure<MemberResponse, Failure>(new Failure
        {
            Code = FailureCode.ValidationError,
            Message = "Email 不可為空",
            TraceId = traceContext.TraceId
        });
    }

    // 呼叫 Repository
    var member = new Member { Email = request.Email, Name = request.Name };
    var result = await memberRepository.CreateAsync(member, cancel);

    // 處理 Repository 回傳的 Result
    if (result.IsFailure)
    {
        return Result.Failure<MemberResponse, Failure>(result.Error);
    }

    // 轉換為 Response DTO
    var response = new MemberResponse
    {
        Id = result.Value.Id,
        Email = result.Value.Email,
        Name = result.Value.Name
    };

    return Result.Success<MemberResponse, Failure>(response);
}
```

### 3. Controller 層錯誤處理

**職責**：將 Result Pattern 轉換為 HTTP 回應

```csharp
[HttpPost]
public async Task<IActionResult> CreateMember(
    [FromBody] CreateMemberRequest request,
    CancellationToken cancel)
{
    var result = await memberHandler.CreateMemberAsync(request, cancel);

    // 使用 Match 處理成功與失敗
    return result.Match(
        onSuccess: member => Ok(member),  // 200 OK
        onFailure: failure =>
        {
            var statusCode = FailureCodeMapper.ToHttpStatusCode(failure.Code);
            return StatusCode(statusCode, new
            {
                error = failure.Code.ToString(),
                message = failure.Message,
                traceId = failure.TraceId
            });
        }
    );
}
```

📝 **Controller 範例**: [src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs](../../src/be/JobBank1111.Job.WebAPI/Member/MemberController.cs)

### 4. ExceptionHandlingMiddleware（系統層級）

**職責**：捕捉未處理的系統例外，統一回應格式

```csharp
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // 記錄例外
            logger.LogError(ex, "未處理的例外發生");

            // 設定回應
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var failure = new Failure
            {
                Code = FailureCode.InternalServerError,
                Message = "伺服器內部錯誤",
                TraceId = context.TraceIdentifier
            };

            await context.Response.WriteAsJsonAsync(failure);
        }
    }
}
```

📝 **實作參考**: [src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs](../../src/be/JobBank1111.Job.WebAPI/ExceptionHandlingMiddleware.cs)

## 錯誤處理最佳實務

### ✅ 應該做的事

1. **使用 Result Pattern 處理業務邏輯錯誤**
   ```csharp
   // ✅ 正確
   return Result.Failure<T, Failure>(new Failure { ... });
   ```

2. **必須保存原始例外**
   ```csharp
   // ✅ 正確：保存 Exception
   Exception = ex
   ```

3. **包含 TraceId**
   ```csharp
   // ✅ 正確：包含追蹤資訊
   TraceId = traceContext.TraceId
   ```

4. **使用 nameof 定義錯誤碼**
   ```csharp
   // ✅ 正確
   Code = FailureCode.DuplicateEmail
   ```

5. **業務邏輯層不記錄錯誤日誌**
   ```csharp
   // ✅ 正確：只回傳 Failure，由 Middleware 記錄日誌
   return Result.Failure<T, Failure>(failure);
   ```

### ❌ 不應該做的事

1. **不要拋出業務邏輯例外**
   ```csharp
   // ❌ 錯誤
   if (member == null)
       throw new NotFoundException("會員不存在");
   ```

2. **不要遺漏原始例外**
   ```csharp
   // ❌ 錯誤：沒有保存 Exception
   return Result.Failure<T, Failure>(new Failure
   {
       Code = FailureCode.DbError,
       Message = "錯誤"
       // ❌ 缺少 Exception = ex
   });
   ```

3. **不要在業務邏輯層記錄錯誤日誌**
   ```csharp
   // ❌ 錯誤
   catch (Exception ex)
   {
       logger.LogError(ex, "錯誤");  // ❌ 業務邏輯層不應記錄日誌
       return Result.Failure<T, Failure>(failure);
   }
   ```

4. **不要洩露內部實作細節**
   ```csharp
   // ❌ 錯誤：洩露 SQL 錯誤訊息給客戶端
   Message = ex.Message  // ❌ 可能包含敏感資訊

   // ✅ 正確：使用友善的錯誤訊息
   Message = "資料庫錯誤，請聯絡管理員"
   ```

5. **不要重複拋出例外**
   ```csharp
   // ❌ 錯誤
   catch (Exception ex)
   {
       logger.LogError(ex, "錯誤");
       throw;  // ❌ 不要重複拋出
   }
   ```

## 常見錯誤場景處理

### 場景 1：資源不存在

```csharp
var member = await dbContext.Members.FindAsync(id, cancel);
if (member == null)
{
    return Result.Failure<Member, Failure>(new Failure
    {
        Code = FailureCode.NotFound,
        Message = $"會員 {id} 不存在",
        TraceId = traceContext.TraceId
    });
}
```

### 場景 2：驗證錯誤

```csharp
if (!IsValidEmail(request.Email))
{
    return Result.Failure<Member, Failure>(new Failure
    {
        Code = FailureCode.ValidationError,
        Message = "Email 格式不正確",
        TraceId = traceContext.TraceId,
        Data = new Dictionary<string, object> { ["Field"] = "Email" }
    });
}
```

### 場景 3：重複資料

```csharp
catch (DbUpdateException ex) when (IsDuplicateKeyError(ex))
{
    return Result.Failure<Member, Failure>(new Failure
    {
        Code = FailureCode.DuplicateEmail,
        Message = "Email 已被使用",
        TraceId = traceContext.TraceId,
        Exception = ex
    });
}
```

### 場景 4：併發衝突

```csharp
catch (DbUpdateConcurrencyException ex)
{
    return Result.Failure<Member, Failure>(new Failure
    {
        Code = FailureCode.DbConcurrency,
        Message = "資料已被其他使用者修改，請重新載入",
        TraceId = traceContext.TraceId,
        Exception = ex
    });
}
```

## 程式碼範本

📝 [failure-template.cs](../assets/failure-template.cs) - Failure 物件範本
📝 [handler-template.cs](../assets/handler-template.cs) - Handler 錯誤處理範本

## 參考資源

- 📚 [CLAUDE.md](../../../CLAUDE.md) - 完整專案指導文件
- 📝 [架構設計](./architecture.md) - 分層架構說明
- 📝 [中介軟體](./middleware.md) - ExceptionHandlingMiddleware 詳解
