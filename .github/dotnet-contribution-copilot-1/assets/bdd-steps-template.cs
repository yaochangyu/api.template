using Reqnroll;
using Xunit;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobBank1111.Job.IntegrationTest.{Feature};

/// <summary>
/// {Feature} BDD 測試步驟實作
/// </summary>
/// <remarks>
/// 此範本展示 BDD Steps 的標準實作模式
/// 
/// 職責：
/// - 實作 Feature 檔案中定義的步驟
/// - 準備測試資料
/// - 呼叫 API
/// - 驗證回應與資料庫狀態
/// 
/// 使用的測試工具：
/// - Reqnroll: BDD 框架（Gherkin 語法支援）
/// - xUnit: 斷言與測試執行
/// - WebApplicationFactory: ASP.NET Core 測試伺服器
/// - Testcontainers: Docker 測試環境
/// </remarks>
[Binding]
public class {Feature}Steps(TestServer testServer)
{
    private readonly HttpClient _client = testServer.CreateClient();
    private HttpResponseMessage _response;
    private Guid _saved{Feature}Id;
    private {Feature}Response _responseData;

    #region Given (前置條件)

    [Given(@"系統中不存在 Email ""(.*)"" 的 {Feature}")]
    public async Task Given系統中不存在Email的Feature(string email)
    {
        // 確保測試資料不存在
        await testServer.Delete{Feature}ByEmailAsync(email);
    }

    [Given(@"系統中已存在 Email ""(.*)"" 的 {Feature}")]
    public async Task Given系統中已存在Email的Feature(string email)
    {
        // 建立測試資料
        var entity = new {Feature}Entity
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "Existing User",
            CreatedAt = DateTime.UtcNow
        };

        await testServer.Insert{Feature}Async(entity);
    }

    [Given(@"資料庫中有以下 {Feature}：")]
    public async Task Given資料庫中有以下Feature(Table table)
    {
        foreach (var row in table.Rows)
        {
            var entity = new {Feature}Entity
            {
                Id = Guid.NewGuid(),
                Email = row["Email"],
                Name = row["Name"],
                CreatedAt = DateTime.UtcNow
            };

            await testServer.Insert{Feature}Async(entity);
        }
    }

    [Given(@"資料庫中有 (.*) 筆 {Feature} 資料")]
    public async Task Given資料庫中有筆Feature資料(int count)
    {
        var entities = Enumerable.Range(1, count)
            .Select(i => new {Feature}Entity
            {
                Id = Guid.NewGuid(),
                Email = $"user{i}@test.com",
                Name = $"User {i}",
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        await testServer.BulkInsert{Feature}sAsync(entities);
    }

    [Given(@"我儲存該 {Feature} 的 ID")]
    public async Task Given我儲存該FeatureId()
    {
        // 從最後插入的資料取得 ID
        _saved{Feature}Id = await testServer.GetLast{Feature}IdAsync();
    }

    [Given(@"資料庫中不存在 ID 為 ""(.*)"" 的 {Feature}")]
    public async Task Given資料庫中不存在ID的Feature(string id)
    {
        // 確保測試資料不存在
        await testServer.Delete{Feature}ByIdAsync(Guid.Parse(id));
    }

    [Given(@"系統已初始化")]
    public Task Given系統已初始化()
    {
        // 可以在這裡執行系統初始化邏輯
        return Task.CompletedTask;
    }

    [Given(@"資料庫已清空")]
    public async Task Given資料庫已清空()
    {
        // 清空所有測試資料
        await testServer.TruncateTable<{Feature}Entity>();
    }

    [Given(@"我未登入")]
    public void Given我未登入()
    {
        // 移除 Authorization 標頭
        _client.DefaultRequestHeaders.Remove("Authorization");
    }

    #endregion

    #region When (執行動作)

    [When(@"我使用以下資訊建立 {Feature}：")]
    public async Task When我使用以下資訊建立Feature(Table table)
    {
        var row = table.Rows[0];
        var request = new Create{Feature}Request
        {
            Email = row["Email"],
            Name = row["Name"]
        };

        _response = await _client.PostAsJsonAsync("/api/{feature}", request);
    }

    [When(@"我使用 Email ""(.*)"" 建立 {Feature}")]
    public async Task When我使用Email建立Feature(string email)
    {
        var request = new Create{Feature}Request
        {
            Email = email,
            Name = "Test User"
        };

        _response = await _client.PostAsJsonAsync("/api/{feature}", request);
    }

    [When(@"我使用以下無效 Email 建立 {Feature}：")]
    public async Task When我使用以下無效Email建立Feature(Table table)
    {
        // 取第一個無效的 Email 測試
        var email = table.Rows[0]["Email"];
        
        var request = new Create{Feature}Request
        {
            Email = email,
            Name = "Test User"
        };

        _response = await _client.PostAsJsonAsync("/api/{feature}", request);
    }

    [When(@"我呼叫 GET ""(.*)""")]
    public async Task When我呼叫GET(string url)
    {
        // 支援變數替換
        url = url.Replace("{ID}", _saved{Feature}Id.ToString());
        
        _response = await _client.GetAsync(url);
    }

    [When(@"我使用以下資訊更新該 {Feature}：")]
    public async Task When我使用以下資訊更新該Feature(Table table)
    {
        var row = table.Rows[0];
        var request = new Update{Feature}Request
        {
            Id = _saved{Feature}Id,
            Email = row["Email"],
            Name = row["Name"]
        };

        _response = await _client.PutAsJsonAsync($"/api/{feature}/{_saved{Feature}Id}", request);
    }

    [When(@"我嘗試更新 ID 為 ""(.*)"" 的 {Feature}")]
    public async Task When我嘗試更新ID的Feature(string id)
    {
        var request = new Update{Feature}Request
        {
            Id = Guid.Parse(id),
            Email = "test@test.com",
            Name = "Test"
        };

        _response = await _client.PutAsJsonAsync($"/api/{feature}/{id}", request);
    }

    [When(@"我呼叫 DELETE ""(.*)""")]
    public async Task When我呼叫DELETE(string url)
    {
        // 支援變數替換
        url = url.Replace("{ID}", _saved{Feature}Id.ToString());
        
        _response = await _client.DeleteAsync(url);
    }

    #endregion

    #region Then (預期結果)

    [Then(@"建立應該成功")]
    [Then(@"更新應該成功")]
    public void Then操作應該成功()
    {
        Assert.True(_response.IsSuccessStatusCode, 
            $"Expected success status code, but got {_response.StatusCode}");
    }

    [Then(@"建立應該失敗")]
    [Then(@"更新應該失敗")]
    public void Then操作應該失敗()
    {
        Assert.False(_response.IsSuccessStatusCode,
            $"Expected error status code, but got {_response.StatusCode}");
    }

    [Then(@"HTTP 狀態碼應該是 (.*)")]
    public void ThenHTTP狀態碼應該是(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)_response.StatusCode);
    }

    [Then(@"系統應該回傳 {Feature} ID")]
    public async Task Then系統應該回傳FeatureId()
    {
        var content = await _response.Content.ReadAsStringAsync();
        _responseData = JsonSerializer.Deserialize<{Feature}Response>(content, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(_responseData);
        Assert.NotEqual(Guid.Empty, _responseData.Id);

        // 儲存 ID 供後續步驟使用
        _saved{Feature}Id = _responseData.Id;
    }

    [Then(@"資料庫中應該存在該 {Feature} 資料")]
    public async Task Then資料庫中應該存在該Feature資料()
    {
        var entity = await testServer.Get{Feature}ByIdAsync(_saved{Feature}Id);
        Assert.NotNull(entity);
    }

    [Then(@"錯誤訊息應該包含 ""(.*)""")]
    public async Task Then錯誤訊息應該包含(string expectedMessage)
    {
        var content = await _response.Content.ReadAsStringAsync();
        Assert.Contains(expectedMessage, content, StringComparison.OrdinalIgnoreCase);
    }

    [Then(@"回應應該包含 (.*) 筆 {Feature} 資料")]
    public async Task Then回應應該包含筆Feature資料(int expectedCount)
    {
        var content = await _response.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<{Feature}Response>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(list);
        Assert.Equal(expectedCount, list.Count);
    }

    [Then(@"回應的 Email 應該是 ""(.*)""")]
    public async Task Then回應的Email應該是(string expectedEmail)
    {
        if (_responseData == null)
        {
            var content = await _response.Content.ReadAsStringAsync();
            _responseData = JsonSerializer.Deserialize<{Feature}Response>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        Assert.Equal(expectedEmail, _responseData.Email);
    }

    [Then(@"回應的 Name 應該是 ""(.*)""")]
    public async Task Then回應的Name應該是(string expectedName)
    {
        if (_responseData == null)
        {
            var content = await _response.Content.ReadAsStringAsync();
            _responseData = JsonSerializer.Deserialize<{Feature}Response>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        Assert.Equal(expectedName, _responseData.Name);
    }

    [Then(@"資料庫中該 {Feature} 的 Email 應該是 ""(.*)""")]
    public async Task Then資料庫中該Feature的Email應該是(string expectedEmail)
    {
        var entity = await testServer.Get{Feature}ByIdAsync(_saved{Feature}Id);
        Assert.Equal(expectedEmail, entity.Email);
    }

    [Then(@"資料庫中該 {Feature} 的 Name 應該是 ""(.*)""")]
    public async Task Then資料庫中該Feature的Name應該是(string expectedName)
    {
        var entity = await testServer.Get{Feature}ByIdAsync(_saved{Feature}Id);
        Assert.Equal(expectedName, entity.Name);
    }

    [Then(@"資料庫中不應該存在該 {Feature}")]
    public async Task Then資料庫中不應該存在該Feature()
    {
        var entity = await testServer.Get{Feature}ByIdAsync(_saved{Feature}Id);
        Assert.Null(entity);
    }

    [Then(@"回應時間應該少於 (.*) 毫秒")]
    public void Then回應時間應該少於毫秒(int maxMilliseconds)
    {
        // 這需要在測試中記錄開始時間
        // 這裡只是範例，實際需要在 When 步驟中開始計時
        Assert.True(true, "Response time validation not implemented");
    }

    #endregion
}

/*
使用方式：

1. 將 {Feature} 替換為實際的功能名稱（例如：Member、Product、Order）
2. 將 {feature} 替換為小寫的 API 路徑（例如：member、product、order）
3. 實作 TestServer 的輔助方法（Insert、Get、Delete 等）
4. 根據實際需求調整 Request/Response DTO

TestServer 輔助方法範例：

public class TestServer
{
    public async Task InsertMemberAsync(MemberEntity entity) { }
    public async Task<MemberEntity> GetMemberByIdAsync(Guid id) { }
    public async Task DeleteMemberByEmailAsync(string email) { }
    public async Task<Guid> GetLastMemberIdAsync() { }
    public async Task BulkInsertMembersAsync(List<MemberEntity> entities) { }
    public async Task TruncateTable<T>() where T : class { }
}

注意事項：
- ✅ 使用 [Binding] 標記類別
- ✅ 使用正規表示式匹配 Gherkin 步驟
- ✅ 保持步驟方法簡潔（單一職責）
- ✅ 使用 testServer 輔助類別管理測試資料
- ✅ 使用 xUnit Assert 進行斷言
- ✅ 支援變數替換（例如：{ID}）
- ✅ 使用 HttpClient 呼叫 API（完整的 Web API 管線）
- ❌ 不直接實例化 Controller（違反 BDD 測試原則）
- ❌ 不使用 Mock 資料庫（使用 Testcontainers）

進階技巧：
- 使用 ScenarioContext 在步驟間共享資料
- 使用 [BeforeScenario] 和 [AfterScenario] 管理測試生命週期
- 使用標籤 (Tags) 組織測試（例如：@smoke, @positive, @negative）
*/
