# TraceContext 設計說明

## 概述

TraceContext 是一個不可變的追蹤上下文物件，用於在整個請求生命週期中傳遞追蹤資訊、使用者身分、請求時間等跨領域資料。

## 設計原則

### 1. 不可變性 (Immutability)

使用 C# `record` 類型與 `init` 關鍵字確保物件建立後無法修改：

```csharp
public sealed record TraceContext
{
    public string TraceId { get; init; } = string.Empty;
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? IpAddress { get; init; }
    public DateTime RequestTime { get; init; }
    
    // 可選：額外的追蹤資訊
    public string? UserAgent { get; init; }
    public string? CorrelationId { get; init; }
    public Dictionary<string, string> CustomProperties { get; init; } = new();
}
```

**優點**：
- ✅ 執行緒安全：多執行緒環境下無需鎖定
- ✅ 可預測性：狀態不會被意外修改
- ✅ 易於測試：可直接建立測試用實例
- ✅ 效能優化：編譯器可進行最佳化

### 2. 依賴注入模式

使用介面隔離原則，分離設定與讀取職責：

```csharp
// 唯讀介面（Handler、Repository 使用）
public interface IContextGetter
{
    TraceContext GetContext();
}

// 可寫入介面（僅 Middleware 使用）
public interface IContextSetter
{
    void SetContext(TraceContext context);
}

// 實作（Scoped 生命週期）
public sealed class TraceContextAccessor : IContextGetter, IContextSetter
{
    private TraceContext? _context;
    
    public TraceContext GetContext() => 
        _context ?? throw new InvalidOperationException("TraceContext 尚未設定");
    
    public void SetContext(TraceContext context) => 
        _context = context ?? throw new ArgumentNullException(nameof(context));
}
```

**注冊方式**：
```csharp
// Program.cs
services.AddScoped<TraceContextAccessor>();
services.AddScoped<IContextGetter>(sp => sp.GetRequiredService<TraceContextAccessor>());
services.AddScoped<IContextSetter>(sp => sp.GetRequiredService<TraceContextAccessor>());
```

### 3. 中介軟體設定

在中介軟體管線早期設定 TraceContext：

```csharp
public sealed class TraceContextMiddleware
{
    private readonly RequestDelegate _next;
    
    public TraceContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(
        HttpContext httpContext, 
        IContextSetter contextSetter,
        ILogger<TraceContextMiddleware> logger)
    {
        // 1. 產生或取得 TraceId
        var traceId = Activity.Current?.Id 
            ?? httpContext.TraceIdentifier 
            ?? Guid.NewGuid().ToString("N");
        
        // 2. 提取使用者資訊（如果已驗證）
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        
        // 3. 提取請求資訊
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        
        // 4. 建立 TraceContext
        var traceContext = new TraceContext
        {
            TraceId = traceId,
            UserId = userId,
            UserName = userName,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CorrelationId = correlationId ?? traceId,
            RequestTime = DateTime.UtcNow
        };
        
        // 5. 設定到 DI 容器（Scoped）
        contextSetter.SetContext(traceContext);
        
        // 6. 記錄請求開始
        logger.LogInformation(
            "Request started: {Method} {Path}, TraceId: {TraceId}, User: {UserId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceContext.TraceId,
            traceContext.UserId ?? "Anonymous");
        
        try
        {
            // 7. 繼續處理請求
            await _next(httpContext);
        }
        finally
        {
            // 8. 記錄請求結束
            var elapsed = DateTime.UtcNow - traceContext.RequestTime;
            logger.LogInformation(
                "Request completed in {ElapsedMs}ms, Status: {StatusCode}",
                elapsed.TotalMilliseconds,
                httpContext.Response.StatusCode);
        }
    }
}
```

**中介軟體註冊**：
```csharp
// Program.cs
var app = builder.Build();

// ⚠️ 必須在驗證中介軟體之後、業務邏輯中介軟體之前
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TraceContextMiddleware>();  // 👈 在這裡註冊

app.MapControllers();
```

## 使用範例

### Handler 中使用

```csharp
public sealed class MemberHandler
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
    
    public async Task<Result<Member>> CreateMemberAsync(
        CreateMemberRequest request,
        CancellationToken ct = default)
    {
        var trace = _contextGetter.GetContext();
        
        // 記錄操作者資訊
        _logger.LogInformation(
            "Creating member, Email: {Email}, RequestedBy: {UserId}, TraceId: {TraceId}",
            request.Email,
            trace.UserId ?? "Anonymous",
            trace.TraceId);
        
        // 建立會員時記錄建立者
        var member = new Member
        {
            Email = request.Email,
            Name = request.Name,
            CreatedBy = trace.UserId ?? "System",
            CreatedAt = trace.RequestTime,
            CreatedFromIp = trace.IpAddress
        };
        
        await _repository.CreateAsync(member, ct);
        
        return Result.Success(member);
    }
}
```

### Repository 中使用（審計追蹤）

```csharp
public sealed class MemberRepository
{
    private readonly JobBankContext _dbContext;
    private readonly IContextGetter _contextGetter;
    
    public async Task<Member> CreateAsync(Member member, CancellationToken ct)
    {
        var trace = _contextGetter.GetContext();
        
        // 自動填入審計欄位
        member.CreatedBy = trace.UserId ?? "System";
        member.CreatedAt = trace.RequestTime;
        
        _dbContext.Members.Add(member);
        await _dbContext.SaveChangesAsync(ct);
        
        return member;
    }
    
    public async Task<Member> UpdateAsync(Member member, CancellationToken ct)
    {
        var trace = _contextGetter.GetContext();
        
        // 自動更新審計欄位
        member.UpdatedBy = trace.UserId ?? "System";
        member.UpdatedAt = trace.RequestTime;
        
        _dbContext.Members.Update(member);
        await _dbContext.SaveChangesAsync(ct);
        
        return member;
    }
}
```

### 結構化日誌整合（Serilog）

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

// 在 Middleware 中加入到 LogContext
public async Task InvokeAsync(HttpContext httpContext, IContextSetter contextSetter)
{
    var traceContext = /* ... 建立 TraceContext ... */;
    contextSetter.SetContext(traceContext);
    
    // 加入到 Serilog LogContext（整個請求週期都會帶上）
    using (LogContext.PushProperty("TraceId", traceContext.TraceId))
    using (LogContext.PushProperty("UserId", traceContext.UserId))
    using (LogContext.PushProperty("IpAddress", traceContext.IpAddress))
    {
        await _next(httpContext);
    }
}
```

## 測試策略

### 單元測試

```csharp
[Fact]
public async Task CreateMember_ShouldRecordCreator()
{
    // Arrange
    var mockContextGetter = new Mock<IContextGetter>();
    mockContextGetter.Setup(x => x.GetContext()).Returns(new TraceContext
    {
        TraceId = "test-trace-id",
        UserId = "user-123",
        RequestTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
    });
    
    var handler = new MemberHandler(
        _mockRepository.Object,
        mockContextGetter.Object,
        _mockLogger.Object);
    
    // Act
    var result = await handler.CreateMemberAsync(new CreateMemberRequest
    {
        Email = "test@example.com",
        Name = "Test User"
    });
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.CreatedBy.Should().Be("user-123");
    result.Value.CreatedAt.Should().Be(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));
}
```

### 整合測試（BDD）

```csharp
[Binding]
public class TraceContextSteps
{
    private readonly TestServer _testServer;
    private readonly HttpClient _client;
    
    [Given(@"我已使用 UserId ""(.*)"" 登入")]
    public void GivenAuthenticatedUser(string userId)
    {
        // 設定測試用 JWT Token
        var token = _testServer.GenerateTestToken(userId);
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    [Then(@"建立的會員的 CreatedBy 應為 ""(.*)""")]
    public async Task ThenCreatedByShouldBe(string expectedUserId)
    {
        var member = await _testServer.GetLastCreatedMemberAsync();
        member.CreatedBy.Should().Be(expectedUserId);
    }
}
```

## 效能考量

### 1. AsyncLocal vs Scoped DI

本專案選擇 **Scoped DI**（而非 AsyncLocal）：

**優點**：
- ✅ 與 ASP.NET Core DI 容器整合完美
- ✅ 生命週期管理自動化
- ✅ 測試友善（容易 Mock）

**缺點**：
- ❌ 每次取得需要解析 DI 容器（但 Scoped 快取已優化）

**替代方案（AsyncLocal）**：
```csharp
public static class TraceContextHolder
{
    private static readonly AsyncLocal<TraceContext?> _context = new();
    
    public static TraceContext? Current
    {
        get => _context.Value;
        set => _context.Value = value;
    }
}
```

### 2. 記憶體使用

TraceContext 是輕量級物件（約 100-200 bytes），每個 HTTP 請求一個實例，GC 壓力極低。

## 常見問題

### Q1: 為什麼不使用 HttpContext.Items？

**回答**：HttpContext.Items 是字典類型，缺乏型別安全，且僅在 Controller 層可存取。TraceContext 透過 DI 注入，可在 Handler、Repository、Service 等所有層使用。

### Q2: TraceId 與 CorrelationId 的差異？

- **TraceId**：單一 HTTP 請求的唯一識別碼（由 ASP.NET Core 或 OpenTelemetry 產生）
- **CorrelationId**：跨多個服務的業務流程識別碼（客戶端傳入，用於分散式追蹤）

### Q3: 是否應該在 TraceContext 加入更多資訊？

**建議**：保持最小化原則，只放跨領域關注點。業務相關資料應透過參數傳遞，不應混入 TraceContext。

## 相關模式

- **Ambient Context Pattern**: TraceContext 是 Ambient Context 的實作
- **Thread-Local Storage**: AsyncLocal 的替代方案
- **Dependency Injection**: Scoped 生命週期管理

## 參考資料

- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [C# Records](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Serilog LogContext](https://github.com/serilog/serilog/wiki/Enrichment#logcontext)
