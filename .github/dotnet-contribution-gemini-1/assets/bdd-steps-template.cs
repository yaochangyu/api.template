using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FluentAssertions;
using Reqnroll; // For [Binding], Given, When, Then attributes and ScenarioContext
using Xunit.Abstractions; // For ITestOutputHelper

// You will likely need to adjust these namespaces based on your project's Test project structure
// and the actual location of your TestServer and ScenarioContext extension methods.
// Example: using YourProjectNamespace.Testing.Common; for TestServer
// Example: using YourProjectNamespace.Testing.Common.ScenarioContextExtensions;

namespace YourProjectNamespace.WebAPI.IntegrationTest.FeatureName; // Adjust namespace as needed

[Binding]
public class FeatureNameSteps : Steps
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _httpClient; // HttpClient from TestServer
    private HttpStatusCode _actualStatusCode;
    private string _actualResponseBody;
    private JsonNode _actualJsonNode;
    private string _currentRequestBody;

    // Constructor to inject ITestOutputHelper and ScenarioContext
    public FeatureNameSteps(ITestOutputHelper testOutputHelper, ScenarioContext scenarioContext)
    {
        _testOutputHelper = testOutputHelper;
        // Assuming TestServer is set up in a [BeforeTestRun] or [BeforeScenario] hook
        // and stored in ScenarioContext. You'll need to adapt this part.
        // For example: _httpClient = scenarioContext.Get<HttpClient>("HttpClient");
        // Or if you have a TestServer instance directly:
        // _httpClient = scenarioContext.Get<TestServer>("TestServerInstance").CreateClient();

        // For this template, we'll assume a TestServer is available via a helper.
        // Replace with your actual TestServer setup.
        var testServer = new TestServer(); // Placeholder: Replace with actual TestServer instance retrieval
        _httpClient = testServer.CreateClient(); 
    }

    [Given(@"應用程式已啟動且健康運行")]
    public void Given應用程式已啟動且健康運行()
    {
        // This step might involve checking a /health endpoint or simply assuming the TestServer is ready
        _httpClient.Should().NotBeNull("HttpClient should be initialized from TestServer.");
        _testOutputHelper.WriteLine("應用程式已啟動且 HttpClient 已準備就緒。");
    }

    [Given(@"我已準備以下 FeatureName 資料")]
    public async Task Given我已準備以下FeatureName資料(Table table)
    {
        // Example: Inserting data directly into the database for setup
        // You would typically get your DbContextFactory from the TestServer's Services
        // await using var dbContext = await _testServer.Services.GetRequiredService<IDbContextFactory<YourDbContextName>>().CreateDbContextAsync();
        // var entities = table.CreateSet<FeatureNameEntity>().ToList(); // Assuming you have a TableExtension for CreateSet
        // await dbContext.Set<FeatureNameEntity>().AddRangeAsync(entities);
        // await dbContext.SaveChangesAsync();
        _testOutputHelper.WriteLine("FeatureName 資料已準備。 (實際資料插入邏輯需實作)");
        await Task.CompletedTask; // Placeholder for async operation
    }

    [Given(@"調用端已準備 FeatureName Request Body 為")]
    public void Given調用端已準備FeatureNameRequestBody為(string requestBody)
    {
        _currentRequestBody = requestBody;
        _testOutputHelper.WriteLine($"Request Body: {_currentRequestBody}");
    }
    
    [When(@"我發送 POST 請求到 ""(.*)""")]
    public async Task When我發送POST請求到(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(_currentRequestBody, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.SendAsync(request);
        _actualStatusCode = response.StatusCode;
        _actualResponseBody = await response.Content.ReadAsStringAsync();
        _actualJsonNode = string.IsNullOrEmpty(_actualResponseBody) ? null : JsonNode.Parse(_actualResponseBody);

        _testOutputHelper.WriteLine($"POST {url} Response Status: {_actualStatusCode}");
        _testOutputHelper.WriteLine($"Response Body: {_actualResponseBody}");
    }

    [When(@"我發送 GET 請求到 ""(.*)""")]
    public async Task When我發送GET請求到(string url)
    {
        var response = await _httpClient.GetAsync(url);
        _actualStatusCode = response.StatusCode;
        _actualResponseBody = await response.Content.ReadAsStringAsync();
        _actualJsonNode = string.IsNullOrEmpty(_actualResponseBody) ? null : JsonNode.Parse(_actualResponseBody);

        _testOutputHelper.WriteLine($"GET {url} Response Status: {_actualStatusCode}");
        _testOutputHelper.WriteLine($"Response Body: {_actualResponseBody}");
    }

    [Then(@"我應該收到 HTTP 狀態碼為 (\d+)")]
    public void Then我應該收到HTTP狀態碼為(int expectedStatusCode)
    {
        _actualStatusCode.Should().Be((HttpStatusCode)expectedStatusCode, $"Expected status code {expectedStatusCode} but got {_actualStatusCode}.");
    }

    [Then(@"回應內容應該是 JSON")]
    public void Then回應內容應該是JSON()
    {
        _actualJsonNode.Should().NotBeNull("Response body should be valid JSON.");
        _testOutputHelper.WriteLine("回應內容是有效的 JSON。");
    }

    [Then(@"回應 JSON 路徑 ""(.*)"" 的值應該是 ""(.*)""")]
    public void Then回應JSON路徑的值應該是(string jsonPath, string expectedValue)
    {
        // Requires Json.Path.JsonNodeExtensions or similar for SelectToken
        var actualValue = _actualJsonNode?.SelectToken(jsonPath)?.ToString(); // SelectToken might be from a custom extension
        actualValue.Should().Be(expectedValue, $"JSON path '{jsonPath}' value mismatch.");
        _testOutputHelper.WriteLine($"JSON path '{jsonPath}' 的值為 '{actualValue}'。");
    }

    [Then(@"回應 JSON 路徑 ""(.*)"" 的長度應該是 (\d+)")]
    public void Then回應JSON路徑的長度應該是(string jsonPath, int expectedLength)
    {
        var array = _actualJsonNode?.SelectToken(jsonPath)?.AsArray();
        array.Should().NotBeNull($"JSON path '{jsonPath}' should point to an array.");
        array.Count.Should().Be(expectedLength, $"Expected array length {expectedLength} but got {array.Count}.");
        _testOutputHelper.WriteLine($"JSON path '{jsonPath}' 的長度為 '{array.Count}'。");
    }

    // Placeholder TestServer class for template completeness
    public class TestServer : IDisposable
    {
        public HttpClient CreateClient() => new HttpClient();
        public IServiceProvider Services { get; } = new ServiceCollection().BuildServiceProvider();
        public void Dispose() { /* cleanup */ }
    }
}

// Example ScenarioContextExtensions (adapt to your actual project)
public static class ScenarioContextExtensions
{
    private const string TestServerInstanceKey = "TestServerInstance";
    // Add other keys as needed

    public static void SetTestServer(this ScenarioContext context, FeatureNameSteps.TestServer server)
    {
        context.Set(server, TestServerInstanceKey);
    }

    public static FeatureNameSteps.TestServer GetTestServer(this ScenarioContext context)
    {
        return context.Get<FeatureNameSteps.TestServer>(TestServerInstanceKey);
    }
    // Add other Get/Set extension methods for UserId, UtcNow, etc.
}
