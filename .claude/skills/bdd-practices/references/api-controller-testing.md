# API 控制器測試指引

## 核心原則

### 強制 BDD 測試策略

**核心規則**：**所有 API 端點測試必須使用 BDD 測試方法，禁止對 Controller 進行單元測試。**

這個規則的背後邏輯：
- API 的價值在於其**外部行為**（HTTP 請求/回應），而非內部實作
- 單獨測試 Controller 實例化會繞過 ASP.NET Core 中介軟體管線
- BDD 測試更接近真實使用情境，測試整個請求流程

### 測試替身優先級

**優先順序**（從高到低）：
1. ✅ **Testcontainers（真實 Docker 容器）**
   - 資料庫（SQL Server、PostgreSQL）
   - 快取（Redis）
   - 優點：完全逼真，發現真實問題

2. ✅ **記憶體內替身**
   - 僅當 Docker 容器無法使用時
   - 例如：某些環境限制、特定平台

3. ❌ **Mock/Stub**（最後手段）
   - 外部 API、第三方服務
   - 不可用於資料庫、快取測試

## BDD vs 單元測試對比

### 為什麼不能單獨測試 Controller

#### ❌ 禁止的模式：Controller 單元測試

```csharp
// ❌ 反模式 - 不要這樣做
[Fact]
public void MemberController_GetById_ShouldReturnOkResult()
{
    // 問題 1：直接實例化 Controller
    var controller = new MemberController(mockHandler);
    
    // 問題 2：繞過中介軟體
    var result = controller.GetById(1);
    
    // 問題 3：測試內部細節而非行為
    Assert.IsType<OkObjectResult>(result);
    
    // 後果：
    // - 無法測試 TraceContext 中介軟體
    // - 無法測試 ExceptionHandlingMiddleware
    // - 無法測試認證/授權管線
    // - 無法測試 HTTP 狀態碼映射
    // - 無法測試回應標頭
}
```

#### ✅ 推薦方式：BDD 整合測試

```csharp
// ✅ 正確 - 使用 BDD 測試
public class GetMemberByIdTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _httpClient;
    
    public async Task InitializeAsync()
    {
        // 使用完整的 Web 應用程式工廠
        _factory = new WebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetMemberById_WhenMemberExists_ReturnsOkWithMember()
    {
        // 優點：
        // ✅ 完整的請求/回應管線
        // ✅ 中介軟體正確執行
        // ✅ 認證/授權流程測試
        // ✅ 真實的 HTTP 狀態碼
        // ✅ 回應標頭測試
        
        var response = await _httpClient.GetAsync("/api/v1/members/1");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsAsync<MemberResponse>();
        Assert.NotNull(body);
    }
    
    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        _factory?.Dispose();
    }
}
```

### 測試方法對比

| 測試方法 | 範圍 | 速度 | 可靠性 | 維護性 | 推薦 |
|---------|------|------|--------|--------|-----|
| **Controller 單元測試** | 單一類別 | 快 | ⚠️ 低 | ❌ 高維護 | ❌ 禁止 |
| **Handler 單元測試** | 業務邏輯 | 快 | ✅ 高 | ✅ 易維護 | ✅ 推薦 |
| **BDD 整合測試** | 完整流程 | 慢 | ✅ 高 | ✅ 易維護 | ✅ 必須 |

## 端到端測試設計

### WebApplicationFactory 使用

#### 基本設置

```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly MsSqlContainer _sqlContainer;
    private readonly RedisContainer _redisContainer;
    
    public HttpClient HttpClient { get; private set; }
    public IDbContextFactory<JobBankDbContext> DbContextFactory { get; private set; }
    
    public async Task InitializeAsync()
    {
        // 1. 啟動 Docker 容器
        _sqlContainer = new MsSqlBuilder()
            .WithPassword("YourStrong@Password")
            .Build();
        
        _redisContainer = new RedisBuilder()
            .Build();
        
        await Task.WhenAll(
            _sqlContainer.StartAsync(),
            _redisContainer.StartAsync()
        );
        
        // 2. 建立 WebApplicationFactory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 替換連線字串
                    services.Configure<SqlConnectionOptions>(opt =>
                    {
                        opt.ConnectionString = _sqlContainer.GetConnectionString();
                    });
                    
                    // 替換 Redis
                    services.AddStackExchangeRedisCache(opt =>
                    {
                        opt.Configuration = _redisContainer.GetConnectionString();
                    });
                });
            });
        
        // 3. 建立 HttpClient
        HttpClient = _factory.CreateClient();
        
        // 4. 執行遷移
        await ApplyMigrationsAsync();
    }
    
    private async Task ApplyMigrationsAsync()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<JobBankDbContext>();
            
            await dbContext.Database.MigrateAsync();
        }
    }
    
    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        _factory?.Dispose();
        
        await Task.WhenAll(
            _sqlContainer.StopAsync(),
            _redisContainer.StopAsync()
        );
    }
}
```

#### 在測試中使用

```csharp
[Collection("Integration Tests")]
public class MemberApiTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    
    public MemberApiTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task CreateMember_WithValidData_Returns201()
    {
        var request = new
        {
            email = "test@example.com",
            name = "Test User"
        };
        
        var response = await _fixture.HttpClient.PostAsJsonAsync(
            "/api/v1/members",
            request
        );
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}
```

## 常見測試情境模式

### 情境 1：成功的資源建立

```gherkin
# feature 檔案
Feature: 建立新會員

  Scenario: 使用有效資料建立會員
    Given 資料庫已初始化
    When 使用者提交有效的會員建立請求
    Then API 回應 201 Created
    And 回應中包含建立的會員資訊
    And 資料庫中確實新增了該會員
```

**實作**：
```csharp
[Given("資料庫已初始化")]
public async Task GivenDatabaseIsInitialized()
{
    // Testcontainers 自動初始化
}

[When("使用者提交有效的會員建立請求")]
public async Task WhenUserSubmitsValidMemberRequest()
{
    var request = new CreateMemberRequest
    {
        Email = "new@example.com",
        Name = "New Member"
    };
    
    _response = await _httpClient.PostAsJsonAsync(
        "/api/v1/members",
        request
    );
}

[Then("API 回應 201 Created")]
public void ThenApiReturns201()
{
    Assert.Equal(HttpStatusCode.Created, _response.StatusCode);
}

[Then("回應中包含建立的會員資訊")]
public async Task ThenResponseContainsMemberInfo()
{
    var content = await _response.Content.ReadAsAsync<MemberResponse>();
    Assert.NotNull(content.Id);
    Assert.Equal("new@example.com", content.Email);
}
```

### 情境 2：驗證失敗

```gherkin
Feature: 會員建立驗證

  Scenario: 重複 Email 建立會員應失敗
    Given 資料庫中已存在 Email "existing@example.com" 的會員
    When 使用者嘗試建立相同 Email 的會員
    Then API 回應 409 Conflict
    And 錯誤訊息指明 Email 已存在
```

**實作**：
```csharp
[Given("資料庫中已存在 Email {string} 的會員")]
public async Task GivenMemberExistsWithEmail(string email)
{
    using (var scope = _factory.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<JobBankDbContext>();
        
        dbContext.Members.Add(new Member { Email = email });
        await dbContext.SaveChangesAsync();
    }
}

[When("使用者嘗試建立相同 Email 的會員")]
public async Task WhenUserTriesToCreateDuplicateEmail()
{
    var request = new { email = "existing@example.com" };
    _response = await _httpClient.PostAsJsonAsync("/api/v1/members", request);
}

[Then("API 回應 409 Conflict")]
public void ThenApiReturns409()
{
    Assert.Equal(HttpStatusCode.Conflict, _response.StatusCode);
}
```

### 情況 3：認證與授權

```gherkin
Feature: 會員資訊存取權限

  Scenario: 未認證使用者無法存取 API
    When 未認證使用者嘗試存取會員詳細資訊
    Then API 回應 401 Unauthorized

  Scenario: 普通會員無法存取他人敏感資訊
    Given 有兩個不同的會員 A 和 B
    When 會員 A 使用其憑證存取會員 B 的敏感資訊
    Then API 回應 403 Forbidden
```

### 情況 4：快取測試

```gherkin
Feature: 快取優化

  Scenario: 重複查詢應使用快取
    Given 快取已清空
    When 首次查詢會員列表
    Then 查詢成功且耗時較長
    When 再次查詢相同會員列表
    Then 查詢成功且耗時較短
    And 來自 Redis 快取
```

**實作**：
```csharp
[When("首次查詢會員列表")]
public async Task WhenFirstQueryMemberList()
{
    _stopwatch.Restart();
    _response1 = await _httpClient.GetAsync("/api/v1/members?page=1");
    _firstQueryTime = _stopwatch.ElapsedMilliseconds;
}

[When("再次查詢相同會員列表")]
public async Task WhenSecondQueryMemberList()
{
    _stopwatch.Restart();
    _response2 = await _httpClient.GetAsync("/api/v1/members?page=1");
    _secondQueryTime = _stopwatch.ElapsedMilliseconds;
}

[Then("查詢成功且耗時較短")]
public void ThenSecondQueryIsFaster()
{
    // 允許 30% 的誤差（測試環境變數）
    Assert.True(_secondQueryTime < _firstQueryTime * 1.3);
}
```

## 測試資料管理

### 方式 1：每個測試獨立容器

```csharp
// 完全隔離，無資料污染
// 缺點：啟動較慢
[Collection("Isolated")]
public class IsolatedTests : IAsyncLifetime
{
    private MsSqlContainer _container;
    
    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder().Build();
        await _container.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}
```

### 方式 2：交易回滾

```csharp
// 快速執行，自動清理
// 優點：執行快，資料隔離
[Fact]
public async Task Test()
{
    using var transaction = await dbContext.Database
        .BeginTransactionAsync();
    
    try
    {
        // 執行測試
        var result = await handler.CreateMemberAsync(...);
        Assert.NotNull(result);
    }
    finally
    {
        // 自動回滾所有變更
        await transaction.RollbackAsync();
    }
}
```

## 測試隔離最佳實踐

### 1. 避免測試順序相依

```csharp
// ❌ 反模式：測試相依
[Fact]
public async Task Test1_CreateMember() { /* ... */ }

[Fact]
public async Task Test2_QueryMember() 
{ 
    // 依賴 Test1 建立的資料
}

// ✅ 正確：每個測試獨立
[Fact]
public async Task CreateMember_ShouldSucceed()
{
    // 自行建立所需資料
}

[Fact]
public async Task QueryMember_ShouldReturnData()
{
    // 自行建立所需資料
}
```

### 2. 使用測試集合共享資源

```csharp
// 同集合內測試共享 WebApplicationFactory
[CollectionDefinition("Integration Tests")]
public class IntegrationTestsCollection : ICollectionFixture<IntegrationTestFixture>
{
    // 所有該集合的測試共享一個 Fixture
}

[Collection("Integration Tests")]
public class Test1 : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    
    public Test1(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### 3. 測試資料清理策略

```csharp
// 在 Teardown 中清理
public async Task DisposeAsync()
{
    using (var scope = _factory.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<JobBankDbContext>();
        
        // 清理測試建立的資料
        dbContext.Members.RemoveRange(
            dbContext.Members.Where(m => m.Email.Contains("test"))
        );
        await dbContext.SaveChangesAsync();
    }
}
```

## 常見問題與答案

### Q1：為什麼不能用 Mock 測試 Controller？

**A**：因為 Mock 無法真實驗證：
- HTTP 狀態碼對應（如 409 Conflict）
- 中介軟體行為（如 TraceContext、認證）
- 回應標頭設置
- 實際的 EF Core 行為

### Q2：如何加速 BDD 測試？

**A**：
1. 使用測試集合共享 WebApplicationFactory
2. 使用交易回滾而非獨立容器
3. 使用 Testcontainers 的並行執行功能
4. 減少不必要的資料庫初始化

### Q3：如何測試非同步控制器？

**A**：HttpClient 內建支援非同步請求，自動等待：

```csharp
var response = await _httpClient.PostAsJsonAsync(
    "/api/v1/members",
    request
);
// HttpClient 自動等待控制器完成
```

## 總結

### API 控制器測試規則

- ✅ **必須使用 BDD 測試**（.feature 檔案 + Reqnroll）
- ✅ **優先使用 Docker 容器**（Testcontainers）
- ✅ **完整的 Web 管線**（WebApplicationFactory）
- ✅ **測試資料隔離**（交易/容器）
- ❌ **禁止 Controller 單元測試**
- ❌ **避免複雜的 Mock**

### 價值體現

- 🎯 測試更貼近真實使用情境
- 🔐 發現中介軟體層面的問題
- 📚 Gherkin 情境即文件
- 🚀 提升回歸測試覆蓋率
