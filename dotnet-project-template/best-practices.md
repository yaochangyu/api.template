# ASP.NET Core 開發指南

> 本文件整合專案規範與最佳實踐，提供從開發到部署的完整指引。
> 適用於 JobBank1111 API 專案及類似的 ASP.NET Core Web API 專案。
>
> **定位**：操作與流程指南（指令、工作流程、環境設定）。
> 編碼原則與程式碼範例請見 [最佳實踐.md](最佳實踐.md)；指令以 [Taskfile.yml](Taskfile.yml) 為準。

---

## 目錄

### 📚 Part 1: 快速開始
- [專案簡介](#專案簡介)
- [快速檢查清單](#快速檢查清單)

### 🛠 Part 2: 開發指導
- [Taskfile 使用原則](#taskfile-使用原則)
- [建置與執行](#建置與執行)
- [EF Core Migrations 規範](#ef-core-migrations-規範)
- [EF Core 反向工程規範](#ef-core-反向工程規範)

### 🏗 Part 3: 架構設計
- [Clean Architecture 分層架構](#clean-architecture-分層架構)
- [C# 現代化特性](#c-現代化特性)
- [依賴注入最佳實務](#依賴注入最佳實務)

### 💡 Part 4: 核心實作
- [Result Pattern 錯誤處理](#result-pattern-錯誤處理)
- [中介軟體管線設計](#中介軟體管線設計)
- [TraceContext 追蹤管理](#tracecontext-追蹤管理)
- [結構化日誌記錄](#結構化日誌記錄)

### 💾 Part 5: 資料存取
- [DbContextFactory 模式](#dbcontextfactory-模式)
- [查詢最佳化](#查詢最佳化)
- [快取策略](#快取策略)

### 🧪 Part 6: 測試策略
- [BDD 開發流程](#bdd-開發流程)
- [Docker 測試環境](#docker-測試環境)
- [測試分層架構](#測試分層架構)

### 🔒 Part 7: 安全性
- [機敏設定管理](#機敏設定管理)
- [敏感資訊過濾](#敏感資訊過濾)
- [安全性防護](#安全性防護)

### 📊 Part 8: 監控與部署
- [健康檢查](#健康檢查)
- [容器化部署](#容器化部署)
- [CI/CD 管線](#cicd-管線)

---

# Part 1: 快速開始

## 專案簡介

這是一個採用 Clean Architecture 模式的 .NET 8.0 Web API 專案，具備以下特性：

### 技術堆疊
- **框架**：ASP.NET Core 8.0
- **資料庫**：Entity Framework Core + SQL Server
- **快取**：Redis (IDistributedCache)
- **錯誤處理**：CSharpFunctionalExtensions (Result Pattern)
- **驗證**：FluentValidation
- **日誌**：Serilog (Console + File + Seq)
- **測試**：xUnit + FluentAssertions + Testcontainers + Reqnroll (BDD)
- **API 文件**：Swagger/OpenAPI + ReDoc + Scalar

### 專案結構
```
src/be/
├── JobBank1111.Job.WebAPI/           # 主要 Web API 專案
│   ├── Controllers/                   # 控制器層
│   ├── Handlers/                      # 業務邏輯處理器
│   └── Repositories/                  # 資料存取儲存庫
├── JobBank1111.Infrastructure/        # 跨領域基礎設施
├── JobBank1111.Job.DB/               # EF Core 資料存取層
├── JobBank1111.Job.Contract/         # API 客戶端合約
├── JobBank1111.Job.Test/             # 單元測試
└── JobBank1111.Job.IntegrationTest/  # 整合測試 (BDD)
```

## 快速檢查清單

### 開發前檢查
- [ ] 已安裝 .NET 8.0 SDK
- [ ] 已安裝 Docker Desktop
- [ ] 已安裝 Task (go-task)
- [ ] 已設定 `env/local.env` 環境變數
- [ ] 已執行 `task dev-init` 初始化開發環境

### 程式碼品質檢查
- [ ] 所有 Repository 和 Handler 方法回傳 `Result<T, Failure>`
- [ ] 使用 Primary Constructor 簡化建構子
- [ ] 使用 `IDbContextFactory<T>` 而非直接注入 DbContext
- [ ] 讀取查詢使用 `AsNoTracking()`
- [ ] 所有 Failure 物件包含 TraceId
- [ ] 中介軟體職責明確，避免重複處理
- [ ] 使用結構化日誌記錄
- [ ] 敏感資訊已過濾，環境區分處理
- [ ] BDD 測試覆蓋所有控制器功能
- [ ] 使用 Docker 容器進行整合測試

---

# Part 2: 開發指導

## Taskfile 使用原則

### 為什麼使用 Taskfile？

**優勢**：
- 命令集中管理，便於維護
- 複雜的多步驟指令簡化為單一命令
- 團隊成員使用一致的開發指令
- 易於整合 CI/CD 管線

**原則**：
- **優先使用 Taskfile**：所有重複執行的開發指令應透過 `task` 命令執行
- **命令集中管理**：複雜的多步驟指令寫入 `Taskfile.yml`
- **提醒與建議**：建議將長指令添加到 Taskfile.yml 供日後重複使用
- **可讀性優先**：任務描述與變數定義應清晰

## 建置與執行

### 開發模式
```bash
# 啟動 API (watch 模式 + --local 參數載入環境變數)
task api-dev

# 建置解決方案
task build

# 執行測試
task test-unit          # 單元測試
task test-integration   # 整合測試
```

### 基礎設施
```bash
# 啟動 Redis
task redis-start

# 啟動 Redis 管理介面
task redis-admin-start

# 初始化開發環境
task dev-init
```

### 程式碼產生
```bash
# 產生 API 客戶端與伺服器端程式碼
task codegen-api

# 僅產生 API 客戶端程式碼
task codegen-api-client

# 僅產生 API 伺服器端程式碼
task codegen-api-server

# 從資料庫反向工程產生 EF Core 實體
task ef-codegen
```

### 文件
```bash
# 產生 API 文件
task codegen-api-doc

# 預覽 API 文件
task codegen-api-preview
```

## EF Core Migrations 規範

### Code First 開發模式

**強制使用 Taskfile**：
- **必須執行**：`task ef-migration-*` 命令
- **禁止直接執行**：不應直接執行 `dotnet ef migrations` 指令
- **原因**：統一管理專案路徑、輸出目錄、連線字串等參數

### 常用指令

```bash
# 建立新的 Migration 檔案
task ef-migration-add MIGRATION_NAME=InitialCreate

# 更新資料庫至最新版本
task ef-database-update

# 其他 migration 操作（回復/移除/清單）目前未納入 Taskfile，
# 需要時於 src/be/JobBank1111.Job.DB 目錄下使用 dotnet ef 對應指令

# 產生 SQL 腳本（FROM → TO）
task ef-migration-script FROM=InitialCreate TO=AddMemberTable
```

### Code First 工作流程

1. **修改 Entity 類別**：在程式碼中修改或建立 Entity 與 DbContext 配置
2. **建立 Migration**：執行 `task ef-migration-add NAME=DescriptiveName`
3. **檢查 Migration**：檢查產生的 Up 與 Down 方法
4. **套用 Migration**：執行 `task ef-database-update`
5. **測試變更**：驗證資料庫結構是否正確
6. **提交版本控制**：提交 Migration 檔案到 Git

### 最佳實務

**✅ 推薦做法**：
- **描述性命名**：Migration 名稱應清楚描述變更（如 `AddMemberEmailIndex`）
- **小步提交**：每次 Migration 專注於單一變更
- **測試先行**：在開發環境測試後才提交
- **SQL 審查**：檢查產生的 SQL 腳本是否符合預期
- **向下相容**：確保 Down 方法能正確回復變更

**❌ 避免做法**：
- 不要在單一 Migration 中混合多個不相關的變更
- 不要直接修改已套用的 Migration 檔案
- 不要跳過 Migration 檢查直接更新生產環境

## EF Core 反向工程規範

### Database First 開發模式

**強制使用 Taskfile**：
- **必須執行**：`task ef-codegen`
- **禁止直接執行**：不應直接執行 `dotnet ef dbcontext scaffold` 指令
- **原因**：統一管理產生參數、自動載入環境變數、確保團隊一致性

### 工作流程

1. **資料庫變更**：在資料庫中建立或修改資料表結構
2. **執行反向工程**：執行 `task ef-codegen` 更新 Entity Model
3. **檢查產生的程式碼**：檢視 Entity 類別與 DbContext
4. **提交版本控制**：提交產生的程式碼到 Git

### Taskfile 範例

```yaml
ef-codegen:
  desc: EF Core 反向工程產生實體
  cmds:
    - task: ef-codegen-member

ef-codegen-member:
  desc: EF Core 反向工程產生 MemberDbContext EF Entities
  dir: "src/be/JobBank1111.Job.DB"
  cmds:
    - dotnet ef dbcontext scaffold "$SYS_DATABASE_CONNECTION_STRING"
        Microsoft.EntityFrameworkCore.SqlServer
        -o AutoGenerated/Entities
        -c MemberDbContext
        --context-dir AutoGenerated/
        -n JobBank1111.Job.DB
        -t Member
        --force
        --no-onconfiguring
        --use-database-names
```

---

# Part 3: 架構設計

## Clean Architecture 分層架構

### 三層架構模式

```
Controller 層 (HTTP 請求/回應處理)
    ↓
Handler 層 (業務邏輯處理)
    ↓
Repository 層 (資料存取)
    ↓
Database (資料儲存)
```

### 職責劃分

#### Controller 層職責

**負責**：
- HTTP 請求/回應映射
- 路由與 HTTP 動詞對應
- 請求模型繫結與驗證
- 結果轉換為 HTTP 回應

**範例**：
```csharp
// MemberV2ControllerImpl.cs
public class MemberV2ControllerImpl(
    MemberHandler memberHandler,
    IHttpContextAccessor httpContextAccessor
) : IMemberController
{
    public async Task<ActionResult<GetMemberResponseCursorPaginatedList>> GetMembersCursorAsync(
        CancellationToken cancellationToken = default)
    {
        var pageSize = TryGetPageSize();
        var nextPageToken = TryGetPageToken();
        var result = await memberHandler.GetMembersCursorAsync(
            pageSize, nextPageToken, true, cancellationToken);

        // 使用 ToActionResult() 統一處理成功/失敗回應
        return result.ToActionResult();
    }

    private int TryGetPageSize()
    {
        var request = httpContextAccessor.HttpContext.Request;
        return request.Headers.TryGetValue("x-page-size", out var pageSize)
            ? int.Parse(pageSize.FirstOrDefault() ?? string.Empty)
            : 10;
    }
}
```

#### Handler 層職責

**負責**：
- 業務邏輯實作與流程協調
- 驗證與業務規則檢查
- 呼叫 Repository 進行資料存取
- 錯誤處理與 Result Pattern 封裝

**範例**：
```csharp
// MemberHandler.cs
public class MemberHandler(
    MemberRepository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<MemberHandler> logger)
{
    public async Task<Result<Member, Failure>> InsertAsync(
        InsertMemberRequest request,
        CancellationToken cancel = default)
    {
        // 1. 檢查是否存在重複資料
        var queryResult = await repository.QueryEmailAsync(request.Email, cancel);
        if (queryResult.IsFailure)
            return queryResult;

        var srcMember = queryResult.Value;

        // 2. 業務規則驗證 (使用 Result Pattern 連續驗證)
        var validateResult = Result.Success<Member, Failure>(srcMember);
        validateResult = ValidateEmail(validateResult, request);
        validateResult = ValidateName(validateResult, request);

        if (validateResult.IsFailure)
            return validateResult;

        // 3. 執行資料寫入
        var insertResult = await repository.InsertAsync(request, cancel);
        if (insertResult.IsFailure)
            return Result.Failure<Member, Failure>(insertResult.Error);

        return Result.Success<Member, Failure>(srcMember);
    }

    private Result<Member, Failure> ValidateEmail(
        Result<Member, Failure> previousResult,
        InsertMemberRequest dest)
    {
        if (previousResult.IsFailure)
            return previousResult;

        var src = previousResult.Value;
        if (src == null)
            return Result.Success<Member, Failure>(src);

        var traceContext = traceContextGetter.Get();
        if (src.Email == dest.Email)
        {
            return Result.Failure<Member, Failure>(new Failure
            {
                Code = nameof(FailureCode.DuplicateEmail),
                Message = "Email 重複",
                Data = src,
                TraceId = traceContext?.TraceId
            });
        }

        return Result.Success<Member, Failure>(src);
    }
}
```

#### Repository 層職責

**負責**：
- EF Core DbContext 操作與查詢封裝
- 資料庫異常處理與轉換為 Result Pattern
- 查詢最佳化（AsNoTracking、TagWith 等）
- 事務管理

**範例**：
```csharp
// MemberRepository.cs
public class MemberRepository(
    ILogger<MemberRepository> logger,
    IContextGetter<TraceContext?> contextGetter,
    IDbContextFactory<MemberDbContext> dbContextFactory,
    TimeProvider timeProvider,
    IUuidProvider uuidProvider,
    IDistributedCache cache,
    JsonSerializerOptions jsonSerializerOptions)
{
    public async Task<Result<int, Failure>> InsertAsync(
        InsertMemberRequest request,
        CancellationToken cancel = default)
    {
        try
        {
            var now = timeProvider.GetUtcNow();
            var traceContext = contextGetter.Get();
            var userId = traceContext?.UserId;

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

            var toDb = new DB.Member
            {
                Id = uuidProvider.NewId(),
                Name = request.Name,
                Age = request.Age,
                Email = request.Email,
                CreatedAt = now,
                CreatedBy = userId,
                ChangedAt = now,
                ChangedBy = userId
            };

            dbContext.Members.Add(toDb);
            var affectedRows = await dbContext.SaveChangesAsync(cancel);

            return Result.Success<int, Failure>(affectedRows);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var traceContext = contextGetter.Get();
            return Result.Failure<int, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbConcurrency),
                Message = "資料衝突，請稍後再試",
                Data = request,
                Exception = ex,
                TraceId = traceContext?.TraceId
            });
        }
        catch (Exception ex)
        {
            var traceContext = contextGetter.Get();
            return Result.Failure<int, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "執行資料庫操作時發生未預期錯誤",
                Data = request,
                Exception = ex,
                TraceId = traceContext?.TraceId
            });
        }
    }
}
```

### 依賴方向原則

**單向依賴，避免循環參考**：
```
Controller → Handler → Repository → Database
```

- Controller 依賴 Handler 介面
- Handler 依賴 Repository 介面
- Repository 依賴 DbContext
- 不允許反向依賴

## C# 現代化特性

### Primary Constructor (C# 12)

**使用 Primary Constructor 簡化建構子與欄位宣告**：

```csharp
// ✅ 推薦：使用 Primary Constructor
public class MemberHandler(
    MemberRepository repository,
    IContextGetter<TraceContext?> traceContextGetter,
    ILogger<MemberHandler> logger)
{
    // 直接使用參數，無需宣告私有欄位
    public async Task<Result<Member, Failure>> InsertAsync(...)
    {
        var result = await repository.InsertAsync(...);
        logger.LogInformation("Member inserted");
        return result;
    }
}

// ❌ 避免：傳統建構子寫法
public class MemberHandler
{
    private readonly MemberRepository _repository;
    private readonly IContextGetter<TraceContext?> _traceContextGetter;
    private readonly ILogger<MemberHandler> _logger;

    public MemberHandler(
        MemberRepository repository,
        IContextGetter<TraceContext?> traceContextGetter,
        ILogger<MemberHandler> logger)
    {
        _repository = repository;
        _traceContextGetter = traceContextGetter;
        _logger = logger;
    }
}
```

### Record 類型用於不可變物件

**使用 record 定義不可變的資料傳輸物件**：

```csharp
// TraceContext.cs
public record TraceContext
{
    public required string TraceId { get; init; }
    public string? UserId { get; init; }
}

// 使用範例
var context = new TraceContext
{
    TraceId = Guid.NewGuid().ToString(),
    UserId = "user123"
};

// 編譯錯誤：無法修改 init-only 屬性
// context.TraceId = "new-id"; // ❌
```

## 依賴注入最佳實務

### 服務註冊

**明確註冊服務，並啟用容器驗證**：

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 啟用依賴注入容器驗證
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;      // 驗證範圍服務
    options.ValidateOnBuild = true;     // 建置時驗證
});

// 註冊服務
builder.Services.AddSingleton(p => JsonSerializeFactory.DefaultOptions);
builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
builder.Services.AddHttpContextAccessor();

// 註冊業務邏輯服務
builder.Services.AddScoped<IMemberController, MemberV2ControllerImpl>();
builder.Services.AddScoped<MemberHandler>();
builder.Services.AddScoped<MemberRepository>();

// 註冊 Context Accessor
builder.Services.AddContextAccessor();
builder.Services.AddScoped<IUuidProvider, UuidProvider>();

// 註冊快取
builder.Services.AddCacheProviderFactory(configuration);

// 註冊資料庫
builder.Services.AddDatabase();
```

### 服務生命週期選擇

| 生命週期 | 使用時機 | 範例 |
|---------|---------|------|
| **Singleton** | 無狀態、執行緒安全、應用程式級別 | `TimeProvider`、`JsonSerializerOptions` |
| **Scoped** | 每個 HTTP 請求一個實例 | `DbContext`、`Handler`、`Repository` |
| **Transient** | 每次注入都建立新實例 | 輕量級工具類別 |

**原則**：
- 預設使用 **Scoped**
- 確定無狀態且執行緒安全時才使用 **Singleton**
- 避免在 Singleton 服務中注入 Scoped 服務

---

# Part 4: 核心實作

## Result Pattern 錯誤處理

### 為什麼使用 Result Pattern？

**傳統例外處理的問題**：
- 效能開銷大
- 控制流程不明確
- 難以區分業務錯誤與系統例外

**Result Pattern 優勢**：
- 明確的成功/失敗處理
- 避免例外處理的效能開銷
- 強制開發者處理錯誤情況

### Failure 物件定義

```csharp
// Failure.cs
public class Failure
{
    /// <summary>錯誤碼</summary>
    public string Code { get; init; } = nameof(FailureCode.Unknown);

    /// <summary>錯誤訊息</summary>
    public string Message { get; init; }

    /// <summary>錯誤發生時的資料</summary>
    public object Data { get; init; }

    /// <summary>追蹤 Id</summary>
    public string TraceId { get; init; }

    /// <summary>例外，不回傳給 Web API</summary>
    [JsonIgnore]
    public Exception Exception { get; init; }

    public List<Failure> Details { get; init; } = new();
}

// FailureCode.cs
public enum FailureCode
{
    Unknown,
    Unauthorized,
    DbError,
    DuplicateEmail,
    DbConcurrency,
    ValidationError,
    InvalidOperation,
    Timeout,
    InternalServerError
}
```

### HTTP 狀態碼映射

```csharp
// FailureCodeMapper.cs
public static class FailureCodeMapper
{
    private static readonly Dictionary<string, HttpStatusCode> CodeMapping = new()
    {
        [nameof(FailureCode.Unauthorized)] = HttpStatusCode.Unauthorized,
        [nameof(FailureCode.DbError)] = HttpStatusCode.InternalServerError,
        [nameof(FailureCode.DuplicateEmail)] = HttpStatusCode.Conflict,
        [nameof(FailureCode.DbConcurrency)] = HttpStatusCode.Conflict,
        [nameof(FailureCode.ValidationError)] = HttpStatusCode.BadRequest
    };

    public static HttpStatusCode GetHttpStatusCode(Failure failure)
    {
        return CodeMapping.TryGetValue(failure.Code, out var statusCode)
            ? statusCode
            : HttpStatusCode.InternalServerError;
    }
}
```

### ActionResult 擴充方法

```csharp
// ActionResult.cs
public class ActionResult<TSuccess, TFailure> : ActionResult
    where TFailure : class
{
    private readonly Result<TSuccess, TFailure> _result;

    public ActionResult(Result<TSuccess, TFailure> result)
    {
        _result = result;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var objectResult = _result.IsSuccess
            ? CreateSuccessResult(_result.Value)
            : CreateFailureResult(_result.Error);

        await objectResult.ExecuteResultAsync(context);
    }

    public ObjectResult CreateSuccessResult(TSuccess value)
    {
        return new ObjectResult(value) { StatusCode = StatusCodes.Status200OK };
    }

    public ObjectResult CreateFailureResult(TFailure error)
    {
        if (error is Failure failure)
        {
            var statusCode = FailureCodeMapper.GetHttpStatusCode(failure);
            return new ObjectResult(error) { StatusCode = (int)statusCode };
        }

        return new ObjectResult(error) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}

// 擴充方法
public static class ResultExtensions
{
    public static ActionResult<TSuccess, TFailure> ToActionResult<TSuccess, TFailure>(
        this Result<TSuccess, TFailure> result)
        where TFailure : class
    {
        return new ActionResult<TSuccess, TFailure>(result);
    }
}
```

### 使用範例

#### Controller 層

```csharp
public async Task<ActionResult<GetMemberResponsePaginatedList>> GetMemberOffsetAsync(
    CancellationToken cancellationToken = default)
{
    var result = await memberHandler.GetMemberOffsetAsync(
        pageIndex, pageSize, noCache, cancellationToken);

    // 一行程式碼統一處理成功/失敗回應
    return result.ToActionResult();
}
```

### 錯誤處理最佳實務

**✅ 推薦做法**：
- 所有 Repository 和 Handler 方法回傳 `Result<T, Failure>`
- 在 catch 區塊中將例外封裝到 `Failure.Exception` 屬性
- 使用 `nameof(FailureCode.*)` 定義錯誤碼
- 確保所有 Failure 物件都包含 TraceId

**❌ 避免做法**：
- 不要在 Handler 層拋出例外（應使用 Result Pattern）
- 不要重複捕捉並重新拋出例外
- 不要在客戶端回應中洩露內部實作細節
- 不要遺漏 TraceId 資訊

## 中介軟體管線設計

### 中介軟體執行順序

**順序非常重要**，越外層的中介軟體越早執行：

```csharp
// Program.cs
app.UseMiddleware<MeasurementMiddleware>();        // 1. 效能監控
app.UseMiddleware<ExceptionHandlingMiddleware>();  // 2. 系統例外處理 (最外層)
app.UseMiddleware<TraceContextMiddleware>();       // 3. 追蹤內容與身分驗證
app.UseMiddleware<RequestParameterLoggerMiddleware>(); // 4. 請求記錄
app.UseAuthorization();                            // 5. 授權
app.UseRouting();                                  // 6. 路由
app.UseEndpoints(...);                             // 7. 端點執行
```

### ExceptionHandlingMiddleware

**職責**：捕捉系統層級例外（非業務邏輯錯誤）

```csharp
// ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        JsonSerializerOptions jsonOptions)
    {
        _next = next;
        _logger = logger;
        _jsonOptions = jsonOptions;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceContext = GetTraceContext(context);

        // 擷取請求資訊用於日誌記錄
        var requestInfo = await RequestInfoExtractor.ExtractRequestInfoAsync(context, _jsonOptions);

        // 記錄未處理的例外
        _logger.LogError(exception,
            "Unhandled exception - {Method} {Path} | TraceId: {TraceId} | RequestInfo: {@RequestInfo}",
            context.Request.Method,
            context.Request.Path,
            traceContext.TraceId,
            requestInfo);

        // 設定回應
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var failure = new Failure
        {
            Code = nameof(FailureCode.Unknown),
            Message = exception.Message,
            TraceId = traceContext.TraceId,
            Exception = exception,
            Data = new { ExceptionType = exception.GetType().Name, Timestamp = DateTimeOffset.UtcNow }
        };

        var jsonResponse = JsonSerializer.Serialize(failure, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}
```

### TraceContextMiddleware

**職責**：追蹤內容管理與使用者身分驗證

```csharp
// TraceContextMiddleware.cs
public class TraceContextMiddleware
{
    private readonly RequestDelegate _next;

    public TraceContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, ILogger<TraceContextMiddleware> logger)
    {
        // 1. 擷取或產生 TraceId
        var traceId = httpContext.Request.Headers[SysHeaderNames.TraceId].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(traceId))
        {
            traceId = httpContext.TraceIdentifier;
        }

        // 2. 身分驗證
        Signin(httpContext);

        if (httpContext.User.Identity.IsAuthenticated == false)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(new Failure
            {
                Code = nameof(FailureCode.Unauthorized),
                Message = "not login"
            });
            return;
        }

        var userId = httpContext.User.Identity.Name;

        // 3. 設定 TraceContext
        var contextSetter = httpContext.RequestServices.GetService<IContextSetter<TraceContext>>();
        contextSetter.Set(new TraceContext { TraceId = traceId, UserId = userId });

        // 4. 附加到日誌範圍
        using var _ = logger.BeginScope("{Location},{TraceId},{UserId}", "TW", traceId, userId);

        // 5. 附加到回應標頭
        httpContext.Response.Headers.TryAdd(SysHeaderNames.TraceId, traceId);

        await _next.Invoke(httpContext);
    }
}
```

### 中介軟體設計原則

**✅ 推薦做法**：
- 每個中介軟體專注於單一職責
- 使用 `await _next(context)` 讓流程自然進行
- 在適當的層級記錄日誌，避免重複

**❌ 避免做法**：
```csharp
// ❌ 避免：攔截例外後再次拋出
try
{
    await _next(context);
}
catch (Exception ex)
{
    _logger.LogError(ex, "錯誤發生");
    throw; // 會造成重複處理
}
```

## TraceContext 追蹤管理

### 集中式管理架構

**TraceContext** 用於在整個請求生命週期中傳遞追蹤資訊（TraceId、UserId）。

### TraceContext 定義

```csharp
// Infrastructure/TraceContext/TraceContext.cs
public record TraceContext
{
    public required string TraceId { get; init; }
    public string? UserId { get; init; }
}
```

### Context Accessor 實作

```csharp
// IContextGetter.cs
public interface IContextGetter<out T>
{
    T Get();
}

// IContextSetter.cs
public interface IContextSetter<in T>
{
    void Set(T value);
}

// ContextAccessor.cs
public class ContextAccessor<T> : IContextGetter<T>, IContextSetter<T>
{
    private static readonly AsyncLocal<T> _current = new();

    public T Get() => _current.Value;

    public void Set(T value) => _current.Value = value;
}
```

### 註冊與使用

#### 服務註冊

```csharp
// Infrastructure/ServiceCollectionExtensions.cs
public static IServiceCollection AddContextAccessor(this IServiceCollection services)
{
    services.AddSingleton<ContextAccessor<TraceContext>>();
    services.AddSingleton<IContextGetter<TraceContext>>(sp =>
        sp.GetRequiredService<ContextAccessor<TraceContext>>());
    services.AddSingleton<IContextSetter<TraceContext>>(sp =>
        sp.GetRequiredService<ContextAccessor<TraceContext>>());
    return services;
}

// Program.cs
builder.Services.AddContextAccessor();
```

#### 在 Repository/Handler 中使用

```csharp
public class MemberRepository(
    IContextGetter<TraceContext?> contextGetter,
    // ... 其他依賴
)
{
    public async Task<Result<int, Failure>> InsertAsync(...)
    {
        try
        {
            var traceContext = contextGetter.Get();
            var userId = traceContext?.UserId;

            var toDb = new DB.Member
            {
                // 使用 UserId 記錄建立者資訊
                CreatedBy = userId,
                ChangedBy = userId
            };
            // ...
        }
        catch (Exception ex)
        {
            var traceContext = contextGetter.Get();
            return Result.Failure<int, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "執行資料庫操作時發生未預期錯誤",
                TraceId = traceContext?.TraceId,  // 附加 TraceId
                Exception = ex
            });
        }
    }
}
```

## 結構化日誌記錄

### Serilog 設定

```csharp
// Program.cs
builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341")  // 日誌伺服器
        .WriteTo.File("logs/aspnet-.txt", rollingInterval: RollingInterval.Minute)
);
```

### 自動附加 TraceId 與 UserId

在 `TraceContextMiddleware` 中使用 `BeginScope`：

```csharp
using var _ = logger.BeginScope("{Location},{TraceId},{UserId}", "TW", traceId, userId);
```

之後所有在該範圍內的日誌都會自動包含這些資訊。

### 結構化日誌範例

```csharp
// ✅ 推薦：結構化日誌
_logger.LogInformation("Creating member with email {Email}", request.Email);
_logger.LogError(ex, "Failed to create member with email {Email}", request.Email);

// 使用 {@Object} 序列化整個物件
_logger.LogError(exception, "Unhandled exception - RequestInfo: {@RequestInfo}", requestInfo);

// ❌ 避免：字串插值
_logger.LogInformation($"Creating member with email {request.Email}");  // 難以查詢
```

### 日誌層級使用建議

| 層級 | 使用時機 | 範例 |
|------|---------|------|
| **Trace** | 詳細追蹤資訊 | 進入/離開方法 |
| **Debug** | 除錯資訊 | 變數值、條件判斷 |
| **Information** | 一般資訊 | 請求完成、業務操作成功 |
| **Warning** | 警告訊息 | 可恢復的錯誤、降級功能 |
| **Error** | 錯誤訊息 | 異常情況、業務邏輯失敗 |
| **Critical** | 嚴重錯誤 | 系統崩潰、資料損毀 |

---

# Part 5: 資料存取

## DbContextFactory 模式

### 為什麼使用 DbContextFactory？

**使用 `IDbContextFactory<T>` 而非直接注入 DbContext**：

```csharp
// ✅ 推薦：DbContextFactory 模式
public class MemberRepository(
    IDbContextFactory<MemberDbContext> dbContextFactory)
{
    public async Task<Result<Member, Failure>> QueryEmailAsync(...)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

        var result = await dbContext.Members
            .Where(p => p.Email == email)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancel);

        return Result.Success<Member, Failure>(result);
    }
}

// ❌ 避免：直接注入 DbContext
public class MemberRepository(MemberDbContext dbContext)
{
    // 可能造成 DbContext 生命週期問題
}
```

**優勢**：
- 更好的控制 DbContext 生命週期
- 避免長時間持有 DbContext
- 支援並行查詢

## 查詢最佳化

### AsNoTracking

**讀取查詢使用 `AsNoTracking()`**：

```csharp
// ✅ 推薦：讀取查詢使用 AsNoTracking
var members = await dbContext.Members
    .Where(p => p.Age > 18)
    .AsNoTracking()  // 不追蹤變更，提升效能
    .ToListAsync(cancel);

// ❌ 避免：讀取查詢追蹤實體（效能損耗）
var members = await dbContext.Members
    .Where(p => p.Age > 18)
    .ToListAsync(cancel);
```

### TagWith 查詢標記

**使用 `TagWith` 標記查詢以便追蹤**：

```csharp
var result = await dbContext.Members
    .Where(p => p.Email == email)
    .TagWith($"{nameof(MemberRepository)}.{nameof(QueryEmailAsync)}({email})")
    .AsNoTracking()
    .FirstOrDefaultAsync(cancel);
```

產生的 SQL：
```sql
-- MemberRepository.QueryEmailAsync(test@example.com)
SELECT [m].[Id], [m].[Name], [m].[Email]
FROM [Members] AS [m]
WHERE [m].[Email] = @__email_0
```

### Select 投影

**避免查詢整個實體，只選取需要的欄位**：

```csharp
// ✅ 推薦：只查詢需要的欄位
var members = await dbContext.Members
    .Where(p => p.Age > 18)
    .Select(p => new GetMemberResponse
    {
        Id = p.Id,
        Name = p.Name,
        Email = p.Email
    })
    .AsNoTracking()
    .ToListAsync(cancel);

// ❌ 避免：查詢整個實體後再轉換
var entities = await dbContext.Members
    .Where(p => p.Age > 18)
    .ToListAsync(cancel);
var members = entities.Select(p => new GetMemberResponse { ... });
```

## 快取策略

### 分散式快取 (Redis)

**使用 `IDistributedCache` 介面**：

```csharp
public class MemberRepository(
    IDistributedCache cache,
    JsonSerializerOptions jsonSerializerOptions,
    // ... 其他依賴
)
{
    public async Task<Result<PaginatedList<GetMemberResponse>, Failure>> GetMemberOffsetAsync(
        int pageIndex, int pageSize, bool noCache = false, CancellationToken cancel = default)
    {
        var key = nameof(CacheKeys.MemberData);

        try
        {
            // 1. 嘗試從快取讀取
            if (noCache == false)
            {
                var cachedData = await cache.GetStringAsync(key, cancel);
                if (cachedData != null)
                {
                    var result = JsonSerializer.Deserialize<PaginatedList<GetMemberResponse>>(
                        cachedData, jsonSerializerOptions);
                    return Result.Success<PaginatedList<GetMemberResponse>, Failure>(result);
                }
            }

            // 2. 從資料庫查詢
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
            var data = await dbContext.Members
                .Select(p => new GetMemberResponse { /* ... */ })
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancel);

            var paginatedResult = new PaginatedList<GetMemberResponse>(data, pageIndex, pageSize, totalCount);

            // 3. 寫入快取
            var serializedData = JsonSerializer.Serialize(paginatedResult, jsonSerializerOptions);
            await cache.SetStringAsync(key, serializedData,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                },
                cancel);

            return Result.Success<PaginatedList<GetMemberResponse>, Failure>(paginatedResult);
        }
        catch (Exception ex)
        {
            // 處理錯誤
        }
    }
}
```

### 快取鍵管理

**集中管理快取鍵**：

```csharp
// CacheKeys.cs
public static class CacheKeys
{
    public const string MemberData = "member:data";
    public const string MemberById = "member:id:{0}";
    public const string MemberByEmail = "member:email:{0}";
}

// 使用
var key = string.Format(CacheKeys.MemberById, memberId);
```

### 快取策略建議

| 資料類型 | TTL 建議 | 快取鍵範例 |
|---------|---------|-----------|
| 靜態設定資料 | 1 小時 ~ 1 天 | `config:settings` |
| 使用者資料 | 5 ~ 30 分鐘 | `user:123` |
| 分頁查詢結果 | 1 ~ 5 分鐘 | `members:page:1:size:10` |
| 即時資料 | 不快取或 < 1 分鐘 | - |

---

# Part 6: 測試策略

## BDD 開發流程

### BDD 測試優先原則

**所有控制器功能必須優先使用 BDD 情境測試**：

```gherkin
# Members.feature
Feature: 會員管理 API
  作為一個 API 用戶
  我想要透過 HTTP 請求管理會員資料

  Scenario: 成功建立新會員
    Given 我有有效的會員建立請求
    When 我發送 POST 請求到 "/api/v1/members"
    Then 回應狀態碼應該是 201 Created
    And 回應內容包含新建立的會員資訊
    And 會員資料已儲存到資料庫中

  Scenario: 建立會員時電子郵件重複
    Given 資料庫中已存在會員使用 "existing@example.com"
    When 我使用相同電子郵件發送 POST 請求到 "/api/v1/members"
    Then 回應狀態碼應該是 409 Conflict
    And 錯誤訊息指出電子郵件地址已被使用
```

### BDD 測試步驟實作

```csharp
// MembersApiSteps.cs
[Binding]
public class MembersApiSteps : BddTestBase
{
    private CreateMemberRequest _createRequest;
    private HttpResponseMessage _response;
    private MemberResponse _memberResponse;

    public MembersApiSteps(DockerTestEnvironment testEnvironment)
        : base(testEnvironment) { }

    [Given(@"我有有效的會員建立請求")]
    public void GivenValidCreateRequest()
    {
        _createRequest = new CreateMemberRequest
        {
            Name = "BDD 測試用戶",
            Email = CreateTestEmail("bdd-test"),
            Phone = "0912345678"
        };
    }

    [When(@"我發送 POST 請求到 ""(.*)""")]
    public async Task WhenPostRequest(string endpoint)
    {
        _response = await Client.PostAsJsonAsync(endpoint, _createRequest);
    }

    [Then(@"回應狀態碼應該是 (\d+) (.*)")]
    public void ThenStatusCodeShouldBe(int statusCode, string statusText)
    {
        ((int)_response.StatusCode).Should().Be(statusCode);
    }

    [Then(@"會員資料已儲存到資料庫中")]
    public async Task ThenMemberStoredInDatabase()
    {
        var verifyResponse = await Client.GetAsync($"/api/v1/members/{_memberResponse.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Docker 測試環境

### 完全基於 Docker 的測試環境

**使用真實的 Docker 容器服務，避免 Mock**：

```csharp
public class DockerTestEnvironment : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServerContainer;
    private readonly RedisContainer _redisContainer;
    private WebApplicationFactory<Program> _factory;

    public DockerTestEnvironment()
    {
        // SQL Server 容器
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("TestPassword123!")
            .WithDatabase("JobBankTestDB")
            .Build();

        // Redis 容器
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // 並行啟動所有容器
        await Task.WhenAll(
            _sqlServerContainer.StartAsync(),
            _redisContainer.StartAsync()
        );

        // 建立 Web 應用程式工廠
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // 使用真實的 Docker SQL Server
                    services.AddDbContext<JobBankDbContext>(options =>
                    {
                        options.UseSqlServer(_sqlServerContainer.GetConnectionString());
                    });

                    // 使用真實的 Docker Redis
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = _redisContainer.GetConnectionString();
                    });
                });
            });

        await InitializeDatabase();
    }

    public HttpClient CreateClient() => _factory.CreateClient();

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await Task.WhenAll(
            _sqlServerContainer.DisposeAsync().AsTask(),
            _redisContainer.DisposeAsync().AsTask()
        );
    }
}
```

## 測試分層架構

### 測試策略分層

| 測試類型 | 涵蓋範圍 | 工具 | 優先級 |
|---------|---------|------|--------|
| **BDD 驗收測試** | 完整的端到端測試 | Reqnroll + Testcontainers | 最高 |
| **整合測試** | 多個元件協作 | xUnit + Docker | 高 |
| **單元測試** | 純函數與業務邏輯 | xUnit + FluentAssertions | 中 |

### 核心原則

**✅ 強制規範**：
- **禁止單獨測試控制器**：必須透過完整的 Web API 管線
- **強制使用 WebApplicationFactory**：確保測試真實的 HTTP 請求處理
- **Docker 優先**：只有在無法使用 Docker 的外部服務才考慮 Mock
- **BDD 優先**：所有新功能都必須先寫 BDD 情境

---

# Part 7: 安全性

## 機敏設定管理

### 機敏設定安全規範

**不應放在 appsettings.json 的資料**：
- 資料庫連線字串
- 帳號密碼
- API Key
- 憑證與金鑰

### 安全來源與環境變數管理

| 環境 | 設定來源 | 範例 |
|------|---------|------|
| **開發環境** | .NET user-secrets + `env/local.env` | `dotnet user-secrets set "ConnectionStrings:Default" "..."` |
| **容器環境** | docker-compose.yml 環境變數或 secrets | `environment:` 或 `secrets:` |
| **雲端/生產** | Azure Key Vault、AWS Secrets Manager | 啟動時載入 |

### 設定覆寫優先順序

```
環境變數 > 使用者機密 > appsettings.{Environment}.json > appsettings.json
```

### 環境變數命名規範

```bash
# 使用雙底線 __ 表示階層
ConnectionStrings__Default=Server=...
ConnectionStrings__Redis=localhost:6379

# 使用前綴避免衝突
JOBBANK_DATABASE_HOST=localhost
JOBBANK_DATABASE_PORT=1433
```

## 敏感資訊過濾

### 記錄日誌時過濾敏感標頭

```csharp
private static readonly string[] SensitiveHeaders =
{
    "Authorization",
    "Cookie",
    "X-API-Key",
    "X-Auth-Token",
    "Set-Cookie",
    "Proxy-Authorization"
};

var headers = context.Request.Headers
    .Where(h => !SensitiveHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
    .ToDictionary(h => h.Key, h => h.Value.ToString());
```

### 環境區分安全策略

```csharp
// 根據環境調整資訊揭露程度
if (env.IsProduction())
{
    // 生產環境：隱藏詳細錯誤資訊
    return new Failure
    {
        Code = nameof(FailureCode.InternalServerError),
        Message = "內部伺服器錯誤",
        TraceId = traceId
    };
}
else
{
    // 開發環境：顯示完整錯誤資訊
    return new Failure
    {
        Code = nameof(FailureCode.Unknown),
        Message = exception.Message,
        TraceId = traceId,
        Data = new { ExceptionType = exception.GetType().Name }
    };
}
```

## 安全性防護

### 客戶端回應安全

**不洩露內部實作細節**：

```csharp
// ✅ 推薦：安全的錯誤回應
public class Failure
{
    public string Code { get; init; }        // "DuplicateEmail"
    public string Message { get; init; }     // "Email 重複"
    public string TraceId { get; init; }     // "abc123" (供追蹤)

    [JsonIgnore]  // 不序列化到客戶端
    public Exception Exception { get; init; }
}

// ❌ 避免：洩露內部細節
{
    "message": "SqlException: Cannot insert duplicate key...",
    "stackTrace": "at System.Data.SqlClient...",
    "connectionString": "Server=prod-db;..."  // 洩露敏感資訊
}
```

### 安全標頭設定

```csharp
// Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'");

    await next();
});
```

---

# Part 8: 監控與部署

## 健康檢查

### 多層健康檢查架構

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "api" })
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        healthQuery: "SELECT 1;",
        name: "database",
        tags: new[] { "database" })
    .AddRedis(
        connectionString: builder.Configuration.GetConnectionString("Redis"),
        name: "redis",
        tags: new[] { "cache" });

// 健康檢查端點
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database") || check.Tags.Contains("cache")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api")
});
```

## 容器化部署

### 多階段建置 Dockerfile

```dockerfile
# 多階段建置 Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 複製專案檔案並還原套件
COPY ["src/be/JobBank1111.Job.WebAPI/JobBank1111.Job.WebAPI.csproj", "JobBank1111.Job.WebAPI/"]
RUN dotnet restore "JobBank1111.Job.WebAPI/JobBank1111.Job.WebAPI.csproj"

# 複製完整原始碼並建置
COPY src/be/ .
RUN dotnet build "JobBank1111.Job.WebAPI/JobBank1111.Job.WebAPI.csproj" -c Release -o /app/build

# 發佈階段
FROM build AS publish
RUN dotnet publish "JobBank1111.Job.WebAPI/JobBank1111.Job.WebAPI.csproj" -c Release -o /app/publish

# 執行時映像
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# 建立非 root 使用者
RUN addgroup -g 1000 appuser && adduser -u 1000 -G appuser -s /bin/sh -D appuser
USER appuser

COPY --from=publish --chown=appuser:appuser /app/publish .

# 健康檢查
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "JobBank1111.Job.WebAPI.dll"]
```

### Docker Compose

```yaml
version: '3.8'

services:
  webapi:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=JobBankDB;...
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      - sqlserver
      - redis
    networks:
      - jobbank-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - jobbank-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    networks:
      - jobbank-network

volumes:
  sqlserver-data:

networks:
  jobbank-network:
    driver: bridge
```

## CI/CD 管線

### GitHub Actions 工作流程

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest

    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: TestPassword123!
        ports:
          - 1433:1433

      redis:
        image: redis:7-alpine
        ports:
          - 6379:6379

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Test
      run: |
        dotnet test src/be/JobBank1111.Job.Test/ --no-build
        dotnet test src/be/JobBank1111.Job.IntegrationTest/ --no-build

  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'

    steps:
    - uses: actions/checkout@v4

    - name: Build and push Docker image
      uses: docker/build-push-action@v4
      with:
        context: .
        push: true
        tags: |
          ghcr.io/${{ github.repository }}:latest
          ghcr.io/${{ github.repository }}:${{ github.sha }}
```

---

## 總結

### 核心原則

1. **Clean Architecture**：明確的分層與職責劃分
2. **Result Pattern**：避免例外處理開銷，明確處理錯誤
3. **TraceContext**：集中管理追蹤資訊
4. **結構化日誌**：自動附加追蹤資訊
5. **DbContextFactory**：更好的生命週期管理
6. **BDD 測試優先**：從使用者角度驗證需求
7. **Taskfile 優先**：統一開發指令
8. **安全性第一**：機敏設定、資訊過濾、環境區分

### 文件使用指引

- **快速開始**：查看快速檢查清單
- **日常開發**：參考 Part 2 開發指導與 Part 3~5 核心實作
- **測試開發**：參考 Part 6 BDD 測試策略
- **部署運維**：參考 Part 8 監控與部署

---

**文件版本**：2.0
**最後更新**：2025-12-15
**整合來源**：CLAUDE.md + 最佳實踐.md
**適用專案**：JobBank1111 API Template
