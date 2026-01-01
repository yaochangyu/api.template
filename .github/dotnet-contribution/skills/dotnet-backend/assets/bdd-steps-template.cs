// BDD Step Definitions Template for API Template
// 使用 Reqnroll (SpecFlow 替代品) 實作測試步驟
// 搭配 Testcontainers 提供真實的 Docker 測試環境

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Xunit;

namespace YourProject.IntegrationTest.Features;

/// <summary>
/// BDD 測試步驟定義範本
/// </summary>
[Binding]
public class FeatureSteps
{
    private readonly TestServer _testServer;
    private readonly HttpClient _client;
    private readonly TestAssistant _assistant;
    
    // 測試過程中的狀態
    private object? _requestData;
    private HttpResponseMessage? _response;
    private string? _responseBody;
    
    public FeatureSteps(TestServer testServer, TestAssistant assistant)
    {
        _testServer = testServer;
        _client = testServer.CreateClient();
        _assistant = assistant;
    }
    
    #region Given - 前置條件
    
    [Given(@"系統已啟動")]
    public void GivenSystemIsRunning()
    {
        // TestServer 已在建構函式初始化
        _testServer.Should().NotBeNull();
    }
    
    [Given(@"資料庫已初始化")]
    public async Task GivenDatabaseIsInitialized()
    {
        await _testServer.EnsureDatabaseCreatedAsync();
    }
    
    [Given(@"我準備註冊會員資料")]
    public void GivenPrepareRegistrationData(Table table)
    {
        var row = table.Rows[0];
        _requestData = new CreateMemberRequest
        {
            Email = row["Email"],
            Name = row["Name"],
            Password = row["Password"]
        };
    }
    
    [Given(@"資料庫中已存在會員")]
    public async Task GivenExistingMemberInDatabase(Table table)
    {
        foreach (var row in table.Rows)
        {
            var member = new Member
            {
                Id = row.ContainsKey("Id") ? row["Id"] : Guid.NewGuid().ToString("N"),
                Email = row["Email"],
                Name = row["Name"],
                CreatedAt = DateTime.UtcNow
            };
            
            await _assistant.SeedMemberAsync(member);
        }
    }
    
    [Given(@"資料庫中不存在 ID 為 ""(.*)"" 的會員")]
    public async Task GivenMemberNotExistsInDatabase(string memberId)
    {
        await _assistant.DeleteMemberAsync(memberId);
    }
    
    #endregion
    
    #region When - 執行動作
    
    [When(@"我發送註冊請求至 ""(.*)""")]
    [When(@"我發送 POST 請求至 ""(.*)""")]
    public async Task WhenSendPostRequest(string endpoint)
    {
        _response = await _client.PostAsJsonAsync(endpoint, _requestData);
        _responseBody = await _response.Content.ReadAsStringAsync();
    }
    
    [When(@"我發送 GET 請求至 ""(.*)""")]
    public async Task WhenSendGetRequest(string endpoint)
    {
        _response = await _client.GetAsync(endpoint);
        _responseBody = await _response.Content.ReadAsStringAsync();
    }
    
    [When(@"我發送 PUT 請求至 ""(.*)""")]
    public async Task WhenSendPutRequest(string endpoint)
    {
        _response = await _client.PutAsJsonAsync(endpoint, _requestData);
        _responseBody = await _response.Content.ReadAsStringAsync();
    }
    
    [When(@"我發送 DELETE 請求至 ""(.*)""")]
    public async Task WhenSendDeleteRequest(string endpoint)
    {
        _response = await _client.DeleteAsync(endpoint);
    }
    
    [When(@"我再次發送 GET 請求至 ""(.*)""")]
    public async Task WhenSendGetRequestAgain(string endpoint)
    {
        // 重置查詢計數器（如果有實作）
        await WhenSendGetRequest(endpoint);
    }
    
    #endregion
    
    #region Then - 驗證結果
    
    [Then(@"回應狀態碼應為 (.*)")]
    public void ThenResponseStatusCodeShouldBe(int expectedCode)
    {
        _response.Should().NotBeNull();
        ((int)_response!.StatusCode).Should().Be(expectedCode);
    }
    
    [Then(@"回應狀態碼應為 200")]
    public void ThenResponseShouldBeOk()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Then(@"回應狀態碼應為 201")]
    public void ThenResponseShouldBeCreated()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Then(@"回應狀態碼應為 400")]
    public void ThenResponseShouldBeBadRequest()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Then(@"回應狀態碼應為 404")]
    public void ThenResponseShouldBeNotFound()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Then(@"回應狀態碼應為 409")]
    public void ThenResponseShouldBeConflict()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
    
    [Then(@"回應標頭 ""(.*)"" 應包含 ""(.*)""")]
    public void ThenResponseHeaderShouldContain(string headerName, string expectedValue)
    {
        _response.Should().NotBeNull();
        _response!.Headers.Should().ContainKey(headerName);
        
        var headerValue = _response.Headers.GetValues(headerName).First();
        headerValue.Should().Contain(expectedValue);
    }
    
    [Then(@"回應內容應包含")]
    public void ThenResponseContentShouldContain(Table table)
    {
        _responseBody.Should().NotBeNullOrEmpty();
        
        foreach (var row in table.Rows)
        {
            var fieldName = row["欄位"];
            var expectedValue = row["值"];
            
            _responseBody.Should().Contain($"\"{fieldName}\"");
            _responseBody.Should().Contain(expectedValue);
        }
    }
    
    [Then(@"錯誤訊息應為 ""(.*)""")]
    public void ThenErrorMessageShouldBe(string expectedMessage)
    {
        _responseBody.Should().NotBeNullOrEmpty();
        _responseBody.Should().Contain(expectedMessage);
    }
    
    [Then(@"錯誤訊息應包含 ""(.*)""")]
    public void ThenErrorMessageShouldContain(string expectedMessage)
    {
        _responseBody.Should().NotBeNullOrEmpty();
        _responseBody.Should().Contain(expectedMessage);
    }
    
    [Then(@"回應中應包含會員 ID")]
    public void ThenResponseShouldContainMemberId()
    {
        _responseBody.Should().NotBeNullOrEmpty();
        _responseBody.Should().MatchRegex(@"""[Ii]d""\s*:\s*""[a-zA-Z0-9]+""");
    }
    
    #endregion
    
    #region Then - 資料庫驗證
    
    [Then(@"資料庫中應存在此會員")]
    public async Task ThenMemberShouldExistInDatabase()
    {
        var request = _requestData as CreateMemberRequest;
        request.Should().NotBeNull();
        
        var member = await _assistant.GetMemberByEmailAsync(request!.Email);
        member.Should().NotBeNull();
        member!.Email.Should().Be(request.Email);
        member.Name.Should().Be(request.Name);
    }
    
    [Then(@"資料庫中應存在 Email 為 ""(.*)"" 的會員")]
    public async Task ThenMemberShouldExistWithEmail(string email)
    {
        var member = await _assistant.GetMemberByEmailAsync(email);
        member.Should().NotBeNull();
        member!.Email.Should().Be(email);
    }
    
    [Then(@"資料庫中應不存在 Email 為 ""(.*)"" 的會員")]
    public async Task ThenMemberShouldNotExistWithEmail(string email)
    {
        var member = await _assistant.GetMemberByEmailAsync(email);
        member.Should().BeNull();
    }
    
    [Then(@"資料庫查詢次數應為 (.*)")]
    public void ThenDatabaseQueryCountShouldBe(int expectedCount)
    {
        // 需要實作查詢計數器（例如：透過自訂 DbCommand Interceptor）
        var actualCount = _assistant.GetDatabaseQueryCount();
        actualCount.Should().Be(expectedCount);
    }
    
    #endregion
    
    #region Then - 快取驗證
    
    [Then(@"Redis 快取中不應存在此會員資料")]
    public async Task ThenRedisShouldNotContainMemberCache()
    {
        var request = _requestData as CreateMemberRequest;
        var member = await _assistant.GetMemberByEmailAsync(request!.Email);
        
        var cacheKey = $"member:{member!.Id}";
        var cached = await _assistant.GetFromRedisAsync(cacheKey);
        cached.Should().BeNull();
    }
    
    [Then(@"Redis 快取應包含 key ""(.*)""")]
    public async Task ThenRedisShouldContainKey(string cacheKey)
    {
        var cached = await _assistant.GetFromRedisAsync(cacheKey);
        cached.Should().NotBeNull();
    }
    
    [Then(@"Redis 查詢次數應為 (.*)")]
    public void ThenRedisQueryCountShouldBe(int expectedCount)
    {
        // 需要實作 Redis 查詢計數器
        var actualCount = _assistant.GetRedisQueryCount();
        actualCount.Should().Be(expectedCount);
    }
    
    #endregion
}

#region 測試輔助類別

/// <summary>
/// 測試輔助工具：提供資料庫、Redis、HTTP 操作的輔助方法
/// </summary>
public class TestAssistant
{
    private readonly TestServer _testServer;
    
    public TestAssistant(TestServer testServer)
    {
        _testServer = testServer;
    }
    
    // 資料庫操作
    public async Task SeedMemberAsync(Member member)
    {
        using var scope = _testServer.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobBankContext>();
        
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task<Member?> GetMemberByEmailAsync(string email)
    {
        using var scope = _testServer.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobBankContext>();
        
        return await dbContext.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Email == email);
    }
    
    public async Task DeleteMemberAsync(string memberId)
    {
        using var scope = _testServer.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobBankContext>();
        
        var member = await dbContext.Members.FindAsync(memberId);
        if (member != null)
        {
            dbContext.Members.Remove(member);
            await dbContext.SaveChangesAsync();
        }
    }
    
    // Redis 操作
    public async Task<string?> GetFromRedisAsync(string key)
    {
        using var scope = _testServer.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        
        return await cache.GetStringAsync(key);
    }
    
    public async Task ClearRedisAsync()
    {
        // 清空 Redis（測試清理用）
        using var scope = _testServer.Services.CreateScope();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        
        var endpoints = redis.GetEndPoints();
        var server = redis.GetServer(endpoints.First());
        await server.FlushDatabaseAsync();
    }
    
    // 查詢計數器（需自訂實作）
    public int GetDatabaseQueryCount() => _testServer.DatabaseQueryCount;
    public int GetRedisQueryCount() => _testServer.RedisQueryCount;
    public void ResetQueryCounters() => _testServer.ResetQueryCounters();
}

#endregion

#region 測試 DTO

public record CreateMemberRequest
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class Member
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

#endregion
