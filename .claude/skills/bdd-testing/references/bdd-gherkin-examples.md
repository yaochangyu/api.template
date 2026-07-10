# BDD Gherkin 語法與 Reqnroll 實作

## Gherkin 語法基礎

### .feature 檔案格式

```gherkin
# language: zh-TW
功能: 會員管理
  作為一個應用使用者
  我想要建立新會員帳戶
  以便能使用該服務

  背景:
    假設資料庫已初始化
    並且未有任何會員記錄

  場景: 成功建立新會員
    當我使用 Email "user@example.com" 建立會員
    那麼應該返回 201 Created
    並且會員資訊已保存到資料庫

  場景: Email 重複時建立失敗
    假設資料庫已存在 Email "duplicate@example.com" 的會員
    當我使用 Email "duplicate@example.com" 建立會員
    那麼應該返回 409 Conflict
    並且錯誤訊息包含 "Email already exists"

  場景: 無效 Email 格式驗證
    當我使用無效 Email "not-an-email" 建立會員
    那麼應該返回 400 Bad Request
    並且錯誤訊息包含驗證錯誤
```

### Gherkin 關鍵字

| 關鍵字 | 用途 | 說明 |
|--------|------|------|
| `功能` / `Feature` | 檔案頂層 | 描述功能名稱與價值主張 |
| `背景` / `Background` | 前置設定 | 所有場景之前執行一次 |
| `場景` / `Scenario` | 單個測試 | 一個具體的測試情況 |
| `假設` / `Given` | 前置條件 | 測試開始時的狀態 |
| `當` / `When` | 動作 | 使用者執行的動作 |
| `那麼` / `Then` | 預期結果 | 應該發生的事情 |
| `並且` / `And` | 延續上面的步驟 | 邏輯AND |
| `但是` / `But` | 否定步驟 | 邏輯但 |

## Reqnroll C# 實作

### 完整步驟定義

```csharp
using Reqnroll;
using System.Net;
using System.Net.Http.Json;

[Binding]
public class MemberStepDefinitions
{
    private readonly HttpClient _httpClient;
    private HttpResponseMessage _lastResponse;
    private Dictionary<string, object> _lastResponseBody;
    
    public MemberStepDefinitions(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    // ========== Given (假設) ==========
    
    [Given("資料庫已初始化")]
    public async Task GivenDatabaseInitialized()
    {
        // 初始化資料庫，建立表格
        var response = await _httpClient.PostAsync(
            "/api/v1/test/setup", 
            null);
        response.EnsureSuccessStatusCode();
    }
    
    [Given("未有任何會員記錄")]
    public async Task GivenNomembersExist()
    {
        // 清空會員表格
        var response = await _httpClient.PostAsync(
            "/api/v1/test/cleanup", 
            null);
        response.EnsureSuccessStatusCode();
    }
    
    [Given("資料庫已存在 Email {0} 的會員")]
    public async Task GivenMemberExistsWith(string email)
    {
        var request = new { Email = email, Name = "Existing User" };
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/members",
            request);
        response.EnsureSuccessStatusCode();
    }
    
    // ========== When (當) ==========
    
    [When("我使用 Email {0} 建立會員")]
    public async Task WhenCreateMember(string email)
    {
        var request = new 
        { 
            Email = email, 
            Name = "Test User" 
        };
        
        _lastResponse = await _httpClient.PostAsJsonAsync(
            "/api/v1/members",
            request);
    }
    
    [When("我使用無效 Email {0} 建立會員")]
    public async Task WhenCreateMemberWithInvalidEmail(string email)
    {
        var request = new 
        { 
            Email = email, 
            Name = "Test User" 
        };
        
        _lastResponse = await _httpClient.PostAsJsonAsync(
            "/api/v1/members",
            request);
    }
    
    // ========== Then (那麼) ==========
    
    [Then("應該返回 (\\d+) (.+)")]
    public void ThenResponseStatus(int statusCode, string description)
    {
        Assert.Equal((HttpStatusCode)statusCode, _lastResponse.StatusCode);
    }
    
    [Then("會員資訊已保存到資料庫")]
    public async Task ThenMemberSavedToDatabase()
    {
        _lastResponse.EnsureSuccessStatusCode();
        
        var response = await _httpClient.GetAsync(
            "/api/v1/members?email=user@example.com");
        
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsAsync<dynamic>();
        Assert.NotNull(content);
    }
    
    [Then("錯誤訊息包含 {0}")]
    public async Task ThenErrorMessageContains(string text)
    {
        _lastResponseBody = await _lastResponse.Content
            .ReadAsAsync<Dictionary<string, object>>();
        
        var message = _lastResponseBody["message"]?.ToString();
        Assert.Contains(text, message);
    }
    
    [Then("錯誤訊息包含驗證錯誤")]
    public async Task ThenErrorMessageContainsValidationError()
    {
        _lastResponseBody = await _lastResponse.Content
            .ReadAsAsync<Dictionary<string, object>>();
        
        Assert.NotNull(_lastResponseBody["data"]["errors"]);
    }
}
```

## 情境設計最佳實踐

### ✅ 良好的情境

```gherkin
場景: 成功建立新會員
  當我使用有效 Email 建立會員
  那麼應該返回 201 Created
  並且會員 ID 已生成
```

**特點**：
- 單一職責（測試一個功能）
- 清晰的期望結果
- 獨立於其他情境

### ❌ 不良的情境

```gherkin
場景: 完整會員功能
  當我建立會員
  並且我更新會員
  並且我刪除會員
  那麼一切都工作正常
```

**問題**：
- 測試多個功能
- 難以定位失敗原因
- 情境相互依賴

## API 端點測試模式

### 模式 1：成功路徑

```gherkin
場景: 成功建立會員
  當我 POST /api/v1/members
    {
      "email": "new@example.com",
      "name": "John Doe"
    }
  那麼應該返回 201
  並且回應包含 id 欄位
```

### 模式 2：驗證失敗

```gherkin
場景: Email 驗證失敗
  當我 POST /api/v1/members
    {
      "email": "invalid-email",
      "name": "John"
    }
  那麼應該返回 400
  並且錯誤代碼為 "ValidationError"
  並且錯誤詳情包含 "email"
```

### 模式 3：業務邏輯失敗

```gherkin
場景: 重複 Email 拒絕
  假設已存在 Email "existing@example.com" 的會員
  當我 POST /api/v1/members
    {
      "email": "existing@example.com",
      "name": "Duplicate"
    }
  那麼應該返回 409
  並且錯誤代碼為 "DuplicateEmail"
```

### 模式 4：權限檢查

```gherkin
場景: 未授權存取拒絕
  當我未經認證 GET /api/v1/members/profile
  那麼應該返回 401
```

## 常見步驟庫

建立可重用的步驟定義：

```csharp
[Binding]
public class CommonSteps
{
    private readonly HttpClient _client;
    
    [When(@"我 (\w+) (.+)")]
    public async Task WhenMakeHttpRequest(string method, string path)
    {
        // 通用 HTTP 請求
    }
    
    [Then(@"應該返回 (\d+)")]
    public void ThenExpectStatus(int code)
    {
        // 通用狀態碼檢查
    }
    
    [Then(@"回應包含 (\w+) 欄位")]
    public void ThenResponseContainsField(string field)
    {
        // 通用欄位檢查
    }
}
```

## 檢查清單

撰寫 .feature 檔案時：
- [ ] 使用清晰的自然語言（非技術人員也能理解）
- [ ] 每個場景測試單一功能
- [ ] Given/When/Then 邏輯清晰
- [ ] 避免 UI 細節，關注業務行為
- [ ] 情境之間無相互依賴
- [ ] 使用背景減少重複
- [ ] 步驟定義保持簡單（複雜邏輯放在 helper 方法）

---

**相關文檔**: Docker Testcontainers 設定、sample.feature 範例
