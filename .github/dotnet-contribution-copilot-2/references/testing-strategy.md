# 測試策略指南

## 測試金字塔

```
      /\
     /  \    E2E Tests (少量)
    /____\
   /      \  Integration Tests (適量)
  /________\
 /          \ Unit Tests (大量)
/__________\
```

## API 測試強制規範

### ✅ 必須使用 BDD 測試
所有 API 端點測試**必須**透過 BDD (Reqnroll) 實作：

```gherkin
Feature: 會員註冊 API
  
  Scenario: 成功註冊新會員
    Given 我準備了有效的註冊資料
    When 我呼叫 POST /api/members 註冊 API
    Then 回應狀態碼應該是 201 Created
    And 回應本文應該包含會員 ID
```

### ❌ 禁止 Controller 單元測試
**不要**直接實例化 Controller 進行測試：

```csharp
// ❌ 錯誤示範
[Fact]
public void CreateMember_ShouldReturnCreated()
{
    var controller = new MemberController(mockHandler.Object);
    var result = controller.CreateMember(request);
    // 無法測試完整的 ASP.NET Core 管線
}
```

### ✅ 必須透過 WebApplicationFactory
**正確**的 API 測試方式：

```csharp
// ✅ 正確示範
public class MemberApiTests : IClassFixture<TestServer>
{
    private readonly HttpClient _client;
    
    public MemberApiTests(TestServer server)
    {
        _client = server.CreateClient();
    }
    
    [Fact]
    public async Task CreateMember_ShouldReturnCreated()
    {
        // 透過真實的 HTTP 請求測試
        var response = await _client.PostAsJsonAsync("/api/members", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## 測試替身策略

### 優先順序

1. **Testcontainers（最優先）**
   - Docker 容器提供真實服務
   - SQL Server、Redis、RabbitMQ 等
   - 避免環境差異問題

2. **In-Memory 實作**
   - 僅在 Testcontainers 不可行時使用
   - 例如：特殊硬體需求的服務

3. **Mock（最後選擇）**
   - 僅用於外部 API、第三方服務
   - 無法用 Docker 模擬的服務

### Testcontainers 設定範例

```csharp
public class TestServer : WebApplicationFactory<Program>
{
    private static readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("YourStrong@Passw0rd")
        .Build();

    private static readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // 替換為測試容器的連線字串
            services.RemoveAll<DbContextOptions<JobBankDbContext>>();
            services.AddDbContext<JobBankDbContext>(options =>
            {
                options.UseSqlServer(_sqlContainer.GetConnectionString());
            });

            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));
        });
    }

    static TestServer()
    {
        // 啟動容器（僅執行一次）
        Task.WaitAll(
            _sqlContainer.StartAsync(),
            _redisContainer.StartAsync()
        );
    }
}
```

## 測試資料管理

### 策略 1: 獨立資料（推薦用於整合測試）

每個測試建立並清理自己的資料：

```csharp
[Binding]
public class MemberSteps
{
    private readonly TestAssistant _assistant;

    [Given(@"系統中存在會員 ""(.*)""")]
    public async Task GivenMemberExists(string email)
    {
        await _assistant.CreateMemberAsync(email);
    }

    [AfterScenario]
    public async Task CleanUp()
    {
        await _assistant.CleanUpTestDataAsync();
    }
}
```

### 策略 2: Seed Data（推薦用於參考資料）

共用的靜態參考資料：

```csharp
[BeforeTestRun]
public static async Task SetupTestData()
{
    await SeedReferenceDataAsync();  // 會員等級、產品分類等
}

[AfterTestRun]
public static async Task TearDown()
{
    await CleanUpTestDatabaseAsync();
}
```

### TestAssistant 範例

```csharp
public class TestAssistant
{
    private readonly JobBankDbContext _context;
    private readonly List<object> _createdEntities = new();

    public async Task<Member> CreateMemberAsync(string email)
    {
        var member = new Member
        {
            Email = email,
            Name = "Test User",
            CreatedAt = DateTime.UtcNow
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        _createdEntities.Add(member);  // 記錄以便清理
        return member;
    }

    public async Task CleanUpTestDataAsync()
    {
        foreach (var entity in _createdEntities)
        {
            _context.Remove(entity);
        }
        await _context.SaveChangesAsync();
        _createdEntities.Clear();
    }
}
```

## 測試範圍決策

### 何時需要測試？

**必須測試**:
- ✅ 核心業務邏輯
- ✅ 複雜的演算法
- ✅ 資料驗證邏輯
- ✅ 安全性相關功能
- ✅ 金流、訂單等關鍵流程

**可以不測試**:
- ⚪ 簡單的 CRUD 操作
- ⚪ 純資料傳輸的 DTO
- ⚪ 自動產生的程式碼
- ⚪ POC 與快速原型

### 測試類型選擇

#### BDD 整合測試（API 層級）
**適用**:
- API 端點測試
- 使用者情境測試
- 端對端流程測試

**範例**:
```gherkin
Scenario: 會員登入後下訂單
  Given 我已經登入為會員 "user@example.com"
  And 購物車中有 2 件商品
  When 我提交訂單
  Then 訂單應該建立成功
  And 購物車應該被清空
  And 我應該收到訂單確認信
```

#### 單元測試（Handler/Repository 層級）
**適用**:
- 複雜業務邏輯
- 邊界條件測試
- 錯誤處理測試

**範例**:
```csharp
[Fact]
public async Task CalculateDiscount_VipMember_ShouldGet20PercentOff()
{
    // Arrange
    var member = new Member { Level = "VIP" };
    var order = new Order { TotalAmount = 1000 };

    // Act
    var discount = _handler.CalculateDiscount(member, order);

    // Assert
    discount.Should().Be(200);
}
```

## 測試互動詢問範本

當需要實作測試時，應詢問用戶：

### 1. 是否需要測試？
- [ ] 是，需要完整測試（BDD + 單元測試）
- [ ] 是，僅需要 BDD 整合測試
- [ ] 是，僅需要單元測試
- [ ] 否，暫不實作測試（快速原型/POC）

### 2. 測試範圍？
- [ ] 新增功能的完整測試
- [ ] 僅測試核心業務邏輯
- [ ] 僅測試 Happy Path
- [ ] 包含異常情境與邊界條件

### 3. BDD 測試情境？（如選擇 BDD）
- [ ] 是否已有 .feature 檔案？
- [ ] 需要新增哪些情境（Given-When-Then）？
- [ ] 需要協助撰寫 Gherkin 語法嗎？

### 4. 測試資料準備？
- [ ] 使用 Testcontainers (Docker)
- [ ] 使用固定 Seed Data
- [ ] 每次測試動態產生
- [ ] 測試後清理資料

### 5. 測試環境？
- [ ] SQL Server (Testcontainers)
- [ ] Redis (Testcontainers)
- [ ] 其他外部服務（需 Mock）

## 測試命名規範

### BDD Feature 檔案
```
功能名稱.feature
例如: MemberRegistration.feature, OrderCreation.feature
```

### BDD Steps 檔案
```
功能名稱Steps.cs
例如: MemberRegistrationSteps.cs, OrderCreationSteps.cs
```

### 單元測試檔案
```
待測類別Tests.cs
例如: MemberHandlerTests.cs, OrderRepositoryTests.cs
```

### 測試方法命名
```
方法名稱_情境_預期結果
例如: CreateMember_DuplicateEmail_ShouldReturnFailure
```

## 測試覆蓋率目標

- **API 端點**: 100%（透過 BDD 測試）
- **Handler 業務邏輯**: 80%+
- **Repository**: 70%+（複雜查詢必須測試）
- **Controller**: 0%（禁止單元測試，由 BDD 覆蓋）

## 常見測試反模式

### ❌ 測試實作細節
```csharp
// 錯誤：測試內部實作
[Fact]
public void Should_Call_Repository_Method()
{
    mockRepo.Verify(x => x.GetById(1), Times.Once);
}
```

### ✅ 測試行為結果
```csharp
// 正確：測試行為與結果
[Fact]
public async Task GetMember_ExistingId_ShouldReturnMember()
{
    var result = await handler.GetMemberAsync(1);
    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(1);
}
```

### ❌ 脆弱的測試
```csharp
// 錯誤：依賴具體日期
[Fact]
public void CreatedDate_Should_Be_20250103()
{
    member.CreatedAt.Should().Be(new DateTime(2025, 1, 3));
}
```

### ✅ 穩定的測試
```csharp
// 正確：測試相對時間
[Fact]
public void CreatedDate_Should_Be_Recent()
{
    member.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 
        TimeSpan.FromSeconds(5));
}
```

## 執行測試指令

```bash
# 執行所有測試
task test

# 僅執行單元測試
task test-unit

# 僅執行整合測試
task test-integration

# 執行特定測試
dotnet test --filter "FullyQualifiedName~MemberRegistration"

# 產生覆蓋率報告
task test-coverage
```

## 參考檔案位置

- TestServer 設定: `src/be/JobBank1111.Job.IntegrationTest/TestServer.cs`
- TestAssistant: `src/be/JobBank1111.Job.IntegrationTest/TestAssistant.cs`
- BDD 範例: `src/be/JobBank1111.Job.IntegrationTest/_01_Demo/`
- 單元測試範例: `src/be/JobBank1111.Job.UnitTest/`
