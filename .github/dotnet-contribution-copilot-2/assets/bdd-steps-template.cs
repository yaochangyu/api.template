using Reqnroll;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace {{NAMESPACE}}.IntegrationTest;

[Binding]
public class {{FEATURE_NAME}}Steps
{
    private readonly HttpClient _client;
    private readonly TestAssistant _assistant;
    private HttpResponseMessage? _response;
    private Create{{FEATURE_NAME}}Request? _request;
    private int _createdId;

    public {{FEATURE_NAME}}Steps(
        TestServer testServer,
        TestAssistant assistant)
    {
        _client = testServer.CreateClient();
        _assistant = assistant;
    }

    // ==================== Given (測試前置條件) ====================

    [Given(@"我是已登入的使用者")]
    public void GivenIAmAuthenticatedUser()
    {
        // TODO: 設定驗證 Token
        // _client.DefaultRequestHeaders.Authorization = 
        //     new AuthenticationHeaderValue("Bearer", _testToken);
    }

    [Given(@"我準備了有效的{{FEATURE_DISPLAY_NAME}}資料")]
    public void GivenIHaveValidData(Table table)
    {
        var data = table.Rows[0];
        _request = new Create{{FEATURE_NAME}}Request
        {
            // TODO: 從 table 對應到 request 屬性
            // Name = data["Name"],
            // Status = data["Status"]
        };
    }

    [Given(@"我準備了不完整的{{FEATURE_DISPLAY_NAME}}資料")]
    public void GivenIHaveIncompleteData(Table table)
    {
        var data = table.Rows[0];
        _request = new Create{{FEATURE_NAME}}Request
        {
            // 故意不設定必填欄位
        };
    }

    [Given(@"系統中已存在 (\d+) 筆{{FEATURE_DISPLAY_NAME}}")]
    public async Task GivenExistingItemsInSystem(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await _assistant.Create{{FEATURE_NAME}}Async($"Item {i + 1}");
        }
    }

    [Given(@"系統中存在 ID 為 (\d+) 的{{FEATURE_DISPLAY_NAME}}")]
    public async Task GivenItemWithIdExists(int id)
    {
        await _assistant.Create{{FEATURE_NAME}}WithIdAsync(id, "Test Item");
    }

    [Given(@"我準備了更新的{{FEATURE_DISPLAY_NAME}}資料")]
    public void GivenIHaveUpdateData(Table table)
    {
        var data = table.Rows[0];
        _request = new Create{{FEATURE_NAME}}Request
        {
            // TODO: 設定更新資料
        };
    }

    [Given(@"我準備了{{FEATURE_DISPLAY_NAME}}資料，其中 (.*) 為 (.*)")]
    public void GivenIHaveDataWithField(string field, string value)
    {
        _request = new Create{{FEATURE_NAME}}Request();
        
        // TODO: 根據 field 設定對應屬性
        // switch (field)
        // {
        //     case "Name":
        //         _request.Name = value;
        //         break;
        //     case "Status":
        //         _request.Status = value;
        //         break;
        // }
    }

    // ==================== When (執行測試動作) ====================

    [When(@"我呼叫 POST /api/{{FEATURE_NAME_LOWER}} API")]
    public async Task WhenICallPostApi()
    {
        _response = await _client.PostAsJsonAsync(
            "/api/{{FEATURE_NAME_LOWER}}", 
            _request);
    }

    [When(@"我呼叫 GET /api/{{FEATURE_NAME_LOWER}} API")]
    public async Task WhenICallGetAllApi()
    {
        _response = await _client.GetAsync("/api/{{FEATURE_NAME_LOWER}}");
    }

    [When(@"我呼叫 GET /api/{{FEATURE_NAME_LOWER}}/(\d+) API")]
    public async Task WhenICallGetByIdApi(int id)
    {
        _response = await _client.GetAsync($"/api/{{FEATURE_NAME_LOWER}}/{id}");
    }

    [When(@"我呼叫 PUT /api/{{FEATURE_NAME_LOWER}}/(\d+) API")]
    public async Task WhenICallPutApi(int id)
    {
        _response = await _client.PutAsJsonAsync(
            $"/api/{{FEATURE_NAME_LOWER}}/{id}", 
            _request);
    }

    [When(@"我呼叫 PUT /api/{{FEATURE_NAME_LOWER}}/(\d+) API 並提供有效資料")]
    public async Task WhenICallPutApiWithValidData(int id)
    {
        _request = new Create{{FEATURE_NAME}}Request
        {
            // TODO: 設定有效資料
        };
        
        _response = await _client.PutAsJsonAsync(
            $"/api/{{FEATURE_NAME_LOWER}}/{id}", 
            _request);
    }

    [When(@"我呼叫 DELETE /api/{{FEATURE_NAME_LOWER}}/(\d+) API")]
    public async Task WhenICallDeleteApi(int id)
    {
        _response = await _client.DeleteAsync($"/api/{{FEATURE_NAME_LOWER}}/{id}");
    }

    [When(@"當我再次呼叫 GET /api/{{FEATURE_NAME_LOWER}}/(\d+) API")]
    public async Task WhenICallGetByIdApiAgain(int id)
    {
        _response = await _client.GetAsync($"/api/{{FEATURE_NAME_LOWER}}/{id}");
    }

    // ==================== Then (驗證測試結果) ====================

    [Then(@"回應狀態碼應該是 (\d+) (.*)")]
    public void ThenResponseStatusCodeShouldBe(int statusCode, string statusText)
    {
        _response.Should().NotBeNull();
        ((int)_response!.StatusCode).Should().Be(statusCode);
    }

    [Then(@"回應本文應該包含 {{FEATURE_NAME}} ID")]
    public async Task ThenResponseShouldContainId()
    {
        var response = await _response!.Content
            .ReadFromJsonAsync<{{FEATURE_NAME}}Response>();
        
        response.Should().NotBeNull();
        response!.Id.Should().BeGreaterThan(0);
        
        _createdId = response.Id;  // 儲存以便後續步驟使用
    }

    [Then(@"回應本文的 (.*) 應該是 ""(.*)""")]
    public async Task ThenResponseFieldShouldBe(string field, string expectedValue)
    {
        var response = await _response!.Content
            .ReadFromJsonAsync<{{FEATURE_NAME}}Response>();
        
        response.Should().NotBeNull();
        
        // TODO: 根據 field 驗證對應屬性
        // switch (field)
        // {
        //     case "Name":
        //         response!.Name.Should().Be(expectedValue);
        //         break;
        //     case "Status":
        //         response!.Status.Should().Be(expectedValue);
        //         break;
        // }
    }

    [Then(@"錯誤訊息應該包含 ""(.*)""")]
    public async Task ThenErrorMessageShouldContain(string expectedMessage)
    {
        var content = await _response!.Content.ReadAsStringAsync();
        content.Should().Contain(expectedMessage, 
            StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"回應本文應該包含 (\d+) 筆資料")]
    public async Task ThenResponseShouldContainItems(int expectedCount)
    {
        var response = await _response!.Content
            .ReadFromJsonAsync<List<{{FEATURE_NAME}}Response>>();
        
        response.Should().NotBeNull();
        response!.Should().HaveCount(expectedCount);
    }

    [Then(@"回應本文的 Id 應該是 (\d+)")]
    public async Task ThenResponseIdShouldBe(int expectedId)
    {
        var response = await _response!.Content
            .ReadFromJsonAsync<{{FEATURE_NAME}}Response>();
        
        response.Should().NotBeNull();
        response!.Id.Should().Be(expectedId);
    }

    // ==================== Hooks (測試生命週期) ====================

    [AfterScenario]
    public async Task CleanUp()
    {
        // 清理此測試建立的資料
        await _assistant.CleanUpTestDataAsync();
    }
}

// ==================== 使用說明 ====================
//
// 此範本提供完整的 BDD 測試步驟實作
//
// 需要替換的變數:
// - {{NAMESPACE}}: 命名空間，例如: JobBank1111.Job
// - {{FEATURE_NAME}}: 功能名稱 (PascalCase)，例如: Member, Order
// - {{FEATURE_NAME_LOWER}}: 功能名稱 (lowercase)，例如: member, order
// - {{FEATURE_DISPLAY_NAME}}: 功能顯示名稱 (中文)，例如: 會員, 訂單
//
// TestAssistant 需要實作的方法:
// - Create{{FEATURE_NAME}}Async(string name)
// - Create{{FEATURE_NAME}}WithIdAsync(int id, string name)
// - CleanUpTestDataAsync()
//
// 自訂建議:
// 1. 根據實際 API 規格調整請求/回應 DTO
// 2. 實作驗證 Token 設定（如需要）
// 3. 新增自訂的 Given/When/Then 步驟
// 4. 加入更多斷言以提高測試覆蓋率
// 5. 考慮使用 ScenarioContext 在步驟間共享資料
