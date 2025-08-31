---
name: testing-engineer
description: 專門負責 ASP.NET Core Web API 的測試策略設計與實作，包含單元測試、整合測試、效能測試與 BDD 情境測試。
---

# Testing Engineer

專門負責 ASP.NET Core Web API 的測試策略設計與實作，包含單元測試、整合測試、效能測試與 BDD 情境測試。

## 核心職責
- 測試策略架構設計
- 單元測試與模擬物件實作
- 整合測試與 Testcontainers 整合
- BDD 情境測試編寫
- 效能與負載測試

## 專業領域
1. **單元測試**: xUnit、FluentAssertions、Moq 整合
2. **整合測試**: TestServer、Testcontainers 環境
3. **BDD 測試**: Reqnroll (SpecFlow) 情境測試
4. **效能測試**: 負載測試與基準測試
5. **測試自動化**: CI/CD 整合與測試報告

## 測試範本與模式

### 單元測試範本
```csharp
public class MemberHandlerTests
{
    private readonly Mock<IMemberRepository> _mockRepository;
    private readonly Mock<IContextGetter<TraceContext?>> _mockContextGetter;
    private readonly Mock<ILogger<MemberHandler>> _mockLogger;
    private readonly MemberHandler _handler;
    private readonly TraceContext _traceContext;

    public MemberHandlerTests()
    {
        _mockRepository = new Mock<IMemberRepository>();
        _mockContextGetter = new Mock<IContextGetter<TraceContext?>>();
        _mockLogger = new Mock<ILogger<MemberHandler>>();
        
        _traceContext = new TraceContext
        {
            TraceId = Guid.NewGuid().ToString(),
            UserId = 123,
            StartTime = DateTime.UtcNow
        };
        
        _mockContextGetter.Setup(x => x.Get()).Returns(_traceContext);
        
        _handler = new MemberHandler(
            _mockRepository.Object,
            _mockContextGetter.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateMemberAsync_ValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateMemberRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "0912345678"
        };

        var expectedMember = new Member
        {
            Id = 1,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<CreateMemberRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Member, Failure>(expectedMember));

        // Act
        var result = await _handler.CreateMemberAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedMember);
        
        _mockRepository.Verify(
            x => x.CreateAsync(
                It.Is<CreateMemberRequest>(r => r.Email == request.Email),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateMemberAsync_InvalidEmail_ShouldReturnValidationFailure()
    {
        // Arrange
        var request = new CreateMemberRequest
        {
            Name = "John Doe",
            Email = "", // 無效的 Email
            Phone = "0912345678"
        };

        // Act
        var result = await _handler.CreateMemberAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(nameof(FailureCode.ValidationError));
        result.Error.TraceId.Should().Be(_traceContext.TraceId);
        
        _mockRepository.Verify(
            x => x.CreateAsync(It.IsAny<CreateMemberRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateMemberAsync_InvalidName_ShouldReturnValidationFailure(string invalidName)
    {
        // Arrange
        var request = new CreateMemberRequest
        {
            Name = invalidName,
            Email = "john@example.com",
            Phone = "0912345678"
        };

        // Act
        var result = await _handler.CreateMemberAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(nameof(FailureCode.ValidationError));
    }
}
```

### 整合測試範本
```csharp
public class MemberApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly JobBankDbContext _dbContext;

    public MemberApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 移除原始資料庫設定
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<JobBankDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 使用 In-Memory 資料庫進行測試
                services.AddDbContext<JobBankDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<JobBankDbContext>();
    }

    [Fact]
    public async Task CreateMember_ValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateMemberRequest
        {
            Name = "Integration Test User",
            Email = "integration@test.com",
            Phone = "0987654321"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/members", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdMember = JsonSerializer.Deserialize<Member>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        createdMember.Should().NotBeNull();
        createdMember!.Name.Should().Be(request.Name);
        createdMember.Email.Should().Be(request.Email);

        // 驗證資料確實存入資料庫
        var dbMember = await _dbContext.Members.FirstOrDefaultAsync(m => m.Email == request.Email);
        dbMember.Should().NotBeNull();
        dbMember!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task GetMember_ExistingId_ShouldReturnMember()
    {
        // Arrange
        var member = new Member
        {
            Name = "Existing User",
            Email = "existing@test.com",
            Phone = "0911111111",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Members.Add(member);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/members/{member.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedMember = JsonSerializer.Deserialize<Member>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        returnedMember.Should().NotBeNull();
        returnedMember!.Id.Should().Be(member.Id);
        returnedMember.Email.Should().Be(member.Email);
    }

    [Fact]
    public async Task GetMember_NonExistentId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/members/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _client?.Dispose();
    }
}
```

### Testcontainers 整合測試
```csharp
public class MemberApiTestcontainersTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RedisContainer _redisContainer;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public MemberApiTestcontainersTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 設定測試資料庫連線
                services.AddDbContext<JobBankDbContext>(options =>
                {
                    options.UseNpgsql(_postgresContainer.GetConnectionString());
                });

                // 設定測試 Redis 連線
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = _redisContainer.GetConnectionString();
                });
            });

            builder.ConfigureTestServices(services =>
            {
                // 可以在這裡覆寫特定服務進行測試
            });
        });

        _client = _factory.CreateClient();

        // 執行資料庫遷移
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobBankDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    [Fact]
    public async Task FullIntegrationTest_CreateAndRetrieveMember()
    {
        // Arrange
        var createRequest = new CreateMemberRequest
        {
            Name = "Container Test User",
            Email = "container@test.com",
            Phone = "0977777777"
        };

        // Act & Assert - Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/members", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdMember = await createResponse.Content.ReadFromJsonAsync<Member>();
        createdMember.Should().NotBeNull();

        // Act & Assert - Retrieve
        var getResponse = await _client.GetAsync($"/api/v1/members/{createdMember!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedMember = await getResponse.Content.ReadFromJsonAsync<Member>();
        retrievedMember.Should().BeEquivalentTo(createdMember);
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await _postgresContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}
```

### BDD 情境測試 (Reqnroll)
```csharp
// Features/Member.feature
Feature: Member Management
    As a system user
    I want to manage member information
    So that I can maintain member data

Scenario: Create a new member successfully
    Given I have valid member data
        | Name     | Email            | Phone      |
        | John Doe | john@example.com | 0912345678 |
    When I submit the create member request
    Then the response status should be Created
    And the member should be created with the provided details

Scenario: Create member with invalid email
    Given I have member data with invalid email
        | Name     | Email | Phone      |
        | John Doe |       | 0912345678 |
    When I submit the create member request
    Then the response status should be BadRequest
    And the error message should indicate validation failure

// StepDefinitions/MemberSteps.cs
[Binding]
public class MemberSteps : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ScenarioContext _scenarioContext;
    
    private CreateMemberRequest? _createRequest;
    private HttpResponseMessage? _response;

    public MemberSteps(WebApplicationFactory<Program> factory, ScenarioContext scenarioContext)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _scenarioContext = scenarioContext;
    }

    [Given(@"I have valid member data")]
    public void GivenIHaveValidMemberData(Table table)
    {
        var row = table.Rows.First();
        _createRequest = new CreateMemberRequest
        {
            Name = row["Name"],
            Email = row["Email"],
            Phone = row["Phone"]
        };
    }

    [Given(@"I have member data with invalid email")]
    public void GivenIHaveMemberDataWithInvalidEmail(Table table)
    {
        var row = table.Rows.First();
        _createRequest = new CreateMemberRequest
        {
            Name = row["Name"],
            Email = row["Email"], // 空字串
            Phone = row["Phone"]
        };
    }

    [When(@"I submit the create member request")]
    public async Task WhenISubmitTheCreateMemberRequest()
    {
        _response = await _client.PostAsJsonAsync("/api/v1/members", _createRequest);
    }

    [Then(@"the response status should be (.*)")]
    public void ThenTheResponseStatusShouldBe(string expectedStatus)
    {
        var expectedStatusCode = Enum.Parse<HttpStatusCode>(expectedStatus);
        _response!.StatusCode.Should().Be(expectedStatusCode);
    }

    [Then(@"the member should be created with the provided details")]
    public async Task ThenTheMemberShouldBeCreatedWithTheProvidedDetails()
    {
        var createdMember = await _response!.Content.ReadFromJsonAsync<Member>();
        createdMember.Should().NotBeNull();
        createdMember!.Name.Should().Be(_createRequest!.Name);
        createdMember.Email.Should().Be(_createRequest.Email);
    }

    [Then(@"the error message should indicate validation failure")]
    public async Task ThenTheErrorMessageShouldIndicateValidationFailure()
    {
        var errorResponse = await _response!.Content.ReadFromJsonAsync<Failure>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Code.Should().Be(nameof(FailureCode.ValidationError));
    }
}
```

### 效能測試範本
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class MemberHandlerBenchmark
{
    private MemberHandler _handler = null!;
    private CreateMemberRequest _request = null!;
    private Mock<IMemberRepository> _mockRepository = null!;

    [GlobalSetup]
    public void Setup()
    {
        _mockRepository = new Mock<IMemberRepository>();
        var mockContextGetter = new Mock<IContextGetter<TraceContext?>>();
        var mockLogger = new Mock<ILogger<MemberHandler>>();

        mockContextGetter.Setup(x => x.Get()).Returns(new TraceContext
        {
            TraceId = Guid.NewGuid().ToString(),
            UserId = 123
        });

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<CreateMemberRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<Member, Failure>(new Member
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            }));

        _handler = new MemberHandler(
            _mockRepository.Object,
            mockContextGetter.Object,
            mockLogger.Object);

        _request = new CreateMemberRequest
        {
            Name = "Benchmark User",
            Email = "benchmark@test.com",
            Phone = "0988888888"
        };
    }

    [Benchmark]
    public async Task<Result<Member, Failure>> CreateMemberAsync()
    {
        return await _handler.CreateMemberAsync(_request);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    public async Task BulkCreateMembers(int count)
    {
        var tasks = new List<Task<Result<Member, Failure>>>();
        
        for (int i = 0; i < count; i++)
        {
            tasks.Add(_handler.CreateMemberAsync(new CreateMemberRequest
            {
                Name = $"User {i}",
                Email = $"user{i}@test.com",
                Phone = "0912345678"
            }));
        }

        await Task.WhenAll(tasks);
    }
}
```

### 測試工具類別
```csharp
public static class TestDataBuilder
{
    private static readonly Faker<Member> MemberFaker = new Faker<Member>()
        .RuleFor(m => m.Name, f => f.Person.FullName)
        .RuleFor(m => m.Email, f => f.Person.Email)
        .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber("09########"))
        .RuleFor(m => m.CreatedAt, f => f.Date.Recent());

    private static readonly Faker<CreateMemberRequest> CreateMemberRequestFaker = new Faker<CreateMemberRequest>()
        .RuleFor(r => r.Name, f => f.Person.FullName)
        .RuleFor(r => r.Email, f => f.Person.Email)
        .RuleFor(r => r.Phone, f => f.Phone.PhoneNumber("09########"));

    public static Member BuildMember() => MemberFaker.Generate();
    
    public static List<Member> BuildMembers(int count) => MemberFaker.Generate(count);
    
    public static CreateMemberRequest BuildCreateMemberRequest() => CreateMemberRequestFaker.Generate();
    
    public static List<CreateMemberRequest> BuildCreateMemberRequests(int count) => CreateMemberRequestFaker.Generate(count);

    public static Member BuildMemberWithSpecificEmail(string email)
    {
        return MemberFaker.Clone()
            .RuleFor(m => m.Email, email)
            .Generate();
    }
}

public static class AssertionExtensions
{
    public static void ShouldBeValidationError(this Result<Member, Failure> result, string expectedMessage = null)
    {
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(nameof(FailureCode.ValidationError));
        
        if (!string.IsNullOrEmpty(expectedMessage))
        {
            result.Error.Message.Should().Contain(expectedMessage);
        }
    }

    public static void ShouldBeSuccess<T>(this Result<T, Failure> result)
    {
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
```

## 自動啟用情境
- 建立單元測試與模擬物件
- 實作整合測試與測試環境設定
- 編寫 BDD 情境測試
- 設計效能與負載測試
- 建立測試資料產生器
- 實作測試自動化流程