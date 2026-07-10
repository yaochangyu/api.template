# TraceContext 管理與生命週期

## TraceContext 設計

`TraceContext` 是一個不可變的記錄類型（C# record），用於在請求的整個生命週期中追蹤和傳遞上下文信息。

### 定義

```csharp
public record TraceContext
{
    /// <summary>
    /// 請求追蹤 ID（用於日誌關聯和問題追蹤）
    /// 格式: GUID 或 HTTP 標頭 X-Trace-Id
    /// </summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>
    /// 當前認證使用者的 ID
    /// 可能為 null（未認證請求）
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// 請求進入時間（UTC）
    /// 用於計算處理耗時
    /// </summary>
    public DateTime RequestTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 使用者角色（可選，用於授權決策）
    /// </summary>
    public string? UserRole { get; init; }

    /// <summary>
    /// 請求來源 IP（用於日誌和安全審計）
    /// </summary>
    public string? ClientIp { get; init; }
}
```

### 不可變性原則

TraceContext 使用 `record` 和 `init` 關鍵字確保不可變性：

```csharp
// ✅ 允許：建立時設定
var context = new TraceContext
{
    TraceId = "123",
    UserId = "user-456"
};

// ❌ 不允許：建立後修改
context.TraceId = "789";  // 編譯錯誤
```

**為什麼要求不可變性？**
- 避免中途修改導致追蹤信息不一致
- 線程安全（AsyncLocal 內）
- 減少副作用和偵錯難度

## 服務注入模式

### IContextSetter 和 IContextGetter

使用依賴注入接口而非直接依賴 TraceContext：

```csharp
/// <summary>
/// 設定上下文（通常在 Middleware 中）
/// </summary>
public interface IContextSetter<T>
{
    void Set(T context);
}

/// <summary>
/// 取得上下文（在業務邏輯層中使用）
/// </summary>
public interface IContextGetter<T>
{
    T Get();
}
```

### 實作使用 AsyncLocal

```csharp
public class TraceContextService : IContextSetter<TraceContext>, IContextGetter<TraceContext>
{
    private static readonly AsyncLocal<TraceContext> _context = new();

    public void Set(TraceContext context)
    {
        _context.Value = context ?? throw new ArgumentNullException(nameof(context));
    }

    public TraceContext Get()
    {
        return _context.Value ?? throw new InvalidOperationException(
            "TraceContext not set. Ensure TraceContextMiddleware is configured.");
    }
}
```

### DI 容器配置

```csharp
services.AddScoped<IContextSetter<TraceContext>>(sp =>
    sp.GetRequiredService<TraceContextService>());
services.AddScoped<IContextGetter<TraceContext>>(sp =>
    sp.GetRequiredService<TraceContextService>());
services.AddScoped<TraceContextService>();
```

## 生命週期流程

### 請求進入

```
HTTP Request
    ↓
TraceContextMiddleware
    ├─ 提取 X-Trace-Id header (或生成新的)
    ├─ 提取 Authorization header (獲取 UserId)
    ├─ 建立不可變 TraceContext 物件
    ├─ 透過 IContextSetter 注入到 AsyncLocal
    └─ 添加 X-Trace-Id 到 Response Header
    ↓
Controller
    ↓
Handler (透過 IContextGetter 取得)
    ↓
Repository (透過 IContextGetter 取得，用於日誌)
    ↓
Database
```

### 實際代碼流程

```csharp
// 1. Middleware 中設定
public class TraceContextMiddleware(IContextSetter<TraceContext> setter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        var userId = ExtractUserId(context);
        
        setter.Set(new TraceContext
        {
            TraceId = traceId,
            UserId = userId,
            ClientIp = context.Connection.RemoteIpAddress?.ToString()
        });
        
        context.Response.Headers.Add("X-Trace-Id", traceId);
        await next(context);
    }
}

// 2. Handler 中使用
public class MemberHandler(
    MemberRepository repo,
    IContextGetter<TraceContext> contextGetter)
{
    public async Task<Result<MemberResponse, Failure>> CreateAsync(
        CreateMemberRequest request)
    {
        var context = contextGetter.Get();  // 取得 TraceContext
        
        var result = await repo.InsertAsync(request.ToEntity());
        if (result.IsFailure)
        {
            // TraceId 用於追蹤此請求的所有操作
            result.Error.TraceId = context.TraceId;
            return Result.Failure<MemberResponse, Failure>(result.Error);
        }
        
        return Result.Success<MemberResponse, Failure>(...);
    }
}

// 3. Repository 中使用（用於日誌和異常處理）
public class MemberRepository(
    IDbContextFactory<AppDbContext> factory,
    IContextGetter<TraceContext> contextGetter,
    ILogger<MemberRepository> logger)
{
    public async Task<Result<Unit, Failure>> InsertAsync(Member member)
    {
        var context = contextGetter.Get();
        
        try
        {
            await using var db = await factory.CreateDbContextAsync();
            db.Members.Add(member);
            await db.SaveChangesAsync();
            
            logger.LogInformation(
                "Member {MemberId} created by user {UserId} (TraceId: {TraceId})",
                member.Id, context.UserId, context.TraceId);
            
            return Result.Success<Unit, Failure>(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to create member for user {UserId} (TraceId: {TraceId})",
                context.UserId, context.TraceId);
            
            return Result.Failure<Unit, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "Failed to create member",
                TraceId = context.TraceId,
                Exception = ex
            });
        }
    }
}
```

## 標頭格式說明

### X-Trace-Id 標頭

**請求標頭**（客戶端發送）：
```
X-Trace-Id: 550e8400-e29b-41d4-a716-446655440000
```

**回應標頭**（伺服器回傳）：
```
X-Trace-Id: 550e8400-e29b-41d4-a716-446655440000
```

客戶端可以使用相同的 TraceId 跨越多個請求進行追蹤。

### 格式選項

1. **UUID/GUID** (推薦)：
   ```
   X-Trace-Id: 550e8400-e29b-41d4-a716-446655440000
   ```

2. **時間戳記 + 序列號**：
   ```
   X-Trace-Id: 20260710T180532Z-001
   ```

3. **自訂格式**：
   ```
   X-Trace-Id: api.template-2026-07-10-12345
   ```

## 常見錯誤與陷阱

### ❌ 錯誤 1：在 Repository 中建立新 TraceContext
```csharp
// 不要這樣做
var context = new TraceContext { TraceId = "..." };  // ❌
```

**原因**：TraceContext 應該在 Middleware 中統一建立並分發。

**改正**：透過 IContextGetter 取得。

### ❌ 錯誤 2：修改 TraceContext（破壞不可變性）
```csharp
context.UserId = "new-user";  // ❌ 編譯錯誤（record 是不可變的）
```

**原因**：不可變性保證追蹤信息的一致性。

**改正**：在 Middleware 中建立時設定所有需要的屬性。

### ❌ 錯誤 3：忘記在異常中包含 TraceId
```csharp
return Result.Failure<T, Failure>(new Failure 
{ 
    Code = "Error",
    Message = "Something failed"
    // ❌ 沒有 TraceId
});
```

**改正**：始終從 TraceContext 中獲取 TraceId 並包含在 Failure 中。

### ❌ 錯誤 4：依賴 TraceContext 在構造函式中設定
```csharp
public class MyService(IContextGetter<TraceContext> getter)
{
    private TraceContext _context = getter.Get();  // ❌ 可能為 null
}
```

**原因**：在非 async 環境中構造函式執行時，AsyncLocal 可能尚未設定。

**改正**：在方法中延遲取得（方法執行時 AsyncLocal 已設定）。

## 檢查清單

設定 TraceContext 時確保：

- [ ] TraceContextMiddleware 位於管線的正確位置（在其他業務 middleware 之前）
- [ ] IContextSetter 和 IContextGetter 都已註冊到 DI 容器
- [ ] 所有 Handler 和 Repository 都使用 IContextGetter 而非直接依賴
- [ ] 回應標頭中包含 X-Trace-Id
- [ ] 所有 Failure 物件都包含 TraceId
- [ ] 日誌中包含 TraceId 便於追蹤
