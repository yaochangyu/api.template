# TraceContext 設計指南

## 什麼是 TraceContext？

TraceContext 是一個不可變的物件，用於在整個請求生命週期中傳遞追蹤資訊與用戶上下文。

## 核心概念

### 不可變物件 (Immutable Object)
使用 C# `record` 類型確保物件一旦建立就不能修改。

```csharp
public record TraceContext
{
    public string RequestId { get; init; }
    public string UserId { get; init; }
    public string UserName { get; init; }
    public string UserRole { get; init; }
    public DateTime RequestTime { get; init; }
    public string ClientIp { get; init; }
}
```

### 為什麼要不可變？
1. **執行緒安全**: 多個執行緒可安全讀取同一個物件
2. **防止意外修改**: 確保追蹤資訊的一致性
3. **易於測試**: 測試時可預期物件狀態不變
4. **易於除錯**: 物件狀態可預測

## 架構設計

### 三層架構
```
TraceContextMiddleware (建立與設定)
    ↓
IContextSetter (設定介面)
    ↓
IContextGetter (取得介面)
    ↓
Handler / Repository (使用)
```

### 介面定義

```csharp
// 設定介面 (僅 Middleware 使用)
public interface IContextSetter
{
    void SetContext(TraceContext context);
}

// 取得介面 (Handler/Repository 使用)
public interface IContextGetter
{
    TraceContext GetContext();
}

// 實作類別
public class TraceContextAccessor : IContextSetter, IContextGetter
{
    private static readonly AsyncLocal<TraceContext?> _context = new();

    public void SetContext(TraceContext context)
    {
        _context.Value = context;
    }

    public TraceContext GetContext()
    {
        return _context.Value 
            ?? throw new InvalidOperationException("TraceContext not set");
    }
}
```

## 中介軟體實作

### TraceContextMiddleware

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
        HttpContext httpContext,
        IContextSetter contextSetter)
    {
        // 建立 TraceContext
        var traceContext = new TraceContext
        {
            RequestId = httpContext.TraceIdentifier,
            UserId = GetUserId(httpContext),
            UserName = GetUserName(httpContext),
            UserRole = GetUserRole(httpContext),
            RequestTime = DateTime.UtcNow,
            ClientIp = GetClientIp(httpContext)
        };

        // 設定到 AsyncLocal 儲存
        contextSetter.SetContext(traceContext);

        // 記錄請求開始
        _logger.LogInformation(
            "Request started: {RequestId}, User: {UserId}",
            traceContext.RequestId,
            traceContext.UserId);

        try
        {
            await _next(httpContext);
        }
        finally
        {
            // 記錄請求結束
            _logger.LogInformation(
                "Request completed: {RequestId}",
                traceContext.RequestId);
        }
    }

    private string GetUserId(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? "anonymous";
    }

    private string GetUserName(HttpContext context)
    {
        return context.User?.Identity?.Name 
            ?? "Anonymous";
    }

    private string GetUserRole(HttpContext context)
    {
        return context.User?.FindFirst(ClaimTypes.Role)?.Value 
            ?? "Guest";
    }

    private string GetClientIp(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() 
            ?? "unknown";
    }
}
```

### 註冊中介軟體

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 註冊服務
builder.Services.AddSingleton<TraceContextAccessor>();
builder.Services.AddSingleton<IContextSetter>(sp => 
    sp.GetRequiredService<TraceContextAccessor>());
builder.Services.AddSingleton<IContextGetter>(sp => 
    sp.GetRequiredService<TraceContextAccessor>());

var app = builder.Build();

// 註冊中介軟體 (必須在驗證中介軟體之後)
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TraceContextMiddleware>();  // ← 這裡

app.MapControllers();
app.Run();
```

## 在 Handler 中使用

```csharp
public class MemberHandler
{
    private readonly MemberRepository _repository;
    private readonly IContextGetter _contextGetter;
    private readonly ILogger<MemberHandler> _logger;

    public MemberHandler(
        MemberRepository repository,
        IContextGetter contextGetter,
        ILogger<MemberHandler> logger)
    {
        _repository = repository;
        _contextGetter = contextGetter;
        _logger = logger;
    }

    public async Task<Result<MemberDto>> GetCurrentMemberAsync()
    {
        // 取得當前用戶資訊
        var context = _contextGetter.GetContext();

        _logger.LogInformation(
            "Getting member info for user {UserId}, Request {RequestId}",
            context.UserId,
            context.RequestId);

        // 使用 UserId 查詢會員資料
        var member = await _repository.GetByUserIdAsync(context.UserId);

        if (member == null)
        {
            return Result.Failure<MemberDto>("Member not found");
        }

        return Result.Success(MapToDto(member));
    }

    public async Task<Result> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var context = _contextGetter.GetContext();

        // 確保用戶只能更新自己的資料
        var member = await _repository.GetByUserIdAsync(context.UserId);
        if (member == null)
        {
            return Result.Failure("Member not found");
        }

        // 記錄操作者資訊
        member.UpdatedBy = context.UserId;
        member.UpdatedAt = DateTime.UtcNow;
        member.UpdatedByIp = context.ClientIp;

        // 執行更新
        await _repository.UpdateAsync(member);

        _logger.LogInformation(
            "Profile updated by {UserId}, Request {RequestId}",
            context.UserId,
            context.RequestId);

        return Result.Success();
    }
}
```

## 在 Repository 中使用

```csharp
public class AuditLogRepository
{
    private readonly JobBankDbContext _context;
    private readonly IContextGetter _contextGetter;

    public AuditLogRepository(
        JobBankDbContext context,
        IContextGetter contextGetter)
    {
        _context = context;
        _contextGetter = contextGetter;
    }

    public async Task LogActionAsync(string action, string details)
    {
        var context = _contextGetter.GetContext();

        var auditLog = new AuditLog
        {
            Action = action,
            Details = details,
            UserId = context.UserId,
            UserName = context.UserName,
            RequestId = context.RequestId,
            ClientIp = context.ClientIp,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
```

## 結構化日誌整合

```csharp
public class MemberHandler
{
    public async Task<Result> CreateMemberAsync(CreateMemberRequest request)
    {
        var context = _contextGetter.GetContext();

        // 使用結構化日誌，包含 TraceContext 資訊
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = context.RequestId,
            ["UserId"] = context.UserId,
            ["UserName"] = context.UserName,
            ["Action"] = "CreateMember"
        }))
        {
            _logger.LogInformation("Creating new member with email {Email}", 
                request.Email);

            var result = await _repository.CreateAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Member created successfully");
            }
            else
            {
                _logger.LogWarning("Failed to create member: {Error}", 
                    result.Error);
            }

            return result;
        }
    }
}
```

## 測試支援

```csharp
public class MemberHandlerTests
{
    [Fact]
    public async Task GetCurrentMember_ShouldReturnMember()
    {
        // Arrange
        var mockContextGetter = new Mock<IContextGetter>();
        mockContextGetter.Setup(x => x.GetContext())
            .Returns(new TraceContext
            {
                RequestId = "test-request-id",
                UserId = "test-user-id",
                UserName = "Test User",
                UserRole = "Member",
                RequestTime = DateTime.UtcNow,
                ClientIp = "127.0.0.1"
            });

        var handler = new MemberHandler(
            repository,
            mockContextGetter.Object,
            logger);

        // Act
        var result = await handler.GetCurrentMemberAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

## 最佳實踐

### ✅ 應該做的
1. 在 Middleware 建立並設定 TraceContext
2. 透過 DI 注入 IContextGetter
3. 使用 record 類型確保不可變
4. 在結構化日誌中包含 TraceContext 資訊
5. 用於審計追蹤與用戶識別

### ❌ 不應該做的
1. 在業務邏輯層直接存取 HttpContext
2. 使用可變的 class 而非 record
3. 在 Controller 中直接使用 IContextSetter
4. 將 TraceContext 傳遞為方法參數（應透過 DI 取得）

## 參考檔案位置

- TraceContext 定義: `src/be/JobBank1111.Job.WebAPI/TraceContext.cs`
- Middleware 實作: `src/be/JobBank1111.Job.WebAPI/TraceContextMiddleware.cs`
- 使用範例: `src/be/JobBank1111.Job.WebAPI/Member/MemberHandler.cs`
