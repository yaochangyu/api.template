# GitHub Copilot Agent 使用範例

本文件提供 `dotnet-api` agent 和 `api-template-bdd-guide` skill 的完整使用範例。

## 📋 目錄

- [環境設定](#環境設定)
- [GitHub Copilot Chat 使用](#github-copilot-chat-使用)
- [GitHub Copilot CLI 使用](#github-copilot-cli-使用)
- [VS Code 使用](#vs-code-使用)
- [實際開發場景範例](#實際開發場景範例)

---

## 環境設定

### 前置需求

1. **GitHub Copilot 訂閱**（個人版或商業版）
2. **VS Code** 或 **Visual Studio 2022**
3. **GitHub Copilot 擴充功能**

### VS Code 安裝步驟

```bash
# 1. 安裝 GitHub Copilot 擴充功能
code --install-extension GitHub.copilot
code --install-extension GitHub.copilot-chat

# 2. 登入 GitHub 帳號
# 在 VS Code 中：Ctrl+Shift+P → "GitHub Copilot: Sign In"
```

### 確認 Agent 已載入

```bash
# 在專案根目錄檢查檔案結構
tree .github\dotnet-contribution /F
```

應該看到：
```
.github\dotnet-contribution\
├── agents\
│   └── dotnet-api.md
└── skills\
    └── api-template-bdd-guide\
        └── ...
```

---

## GitHub Copilot Chat 使用

### 方法 1：在 VS Code 中使用 Chat 面板

#### 1. 開啟 Copilot Chat
- 快捷鍵：`Ctrl + Shift + I` (Windows) 或 `Cmd + Shift + I` (Mac)
- 或點擊側邊欄的 Copilot Chat 圖示

#### 2. 使用 Agent（@mention）

```
@dotnet-api 我要實作會員註冊功能
```

**Agent 會回應**：
```markdown
我會協助你實作會員註冊功能。首先讓我確認幾個問題：

**1. API 開發方式**
1️⃣ API First（推薦）：先定義 OpenAPI 規格
2️⃣ Code First：直接實作程式碼

**2. 需要實作哪些層？**
□ Controller（HTTP 請求處理）
□ Handler（業務邏輯）
□ Repository（資料存取）

**3. 測試需求**
1️⃣ 完整測試（BDD + 單元測試）
2️⃣ 僅 BDD 整合測試
3️⃣ 暫不實作測試

請告訴我你的選擇。
```

#### 3. 使用 Skill（#reference）

```
#api-template-bdd-guide 如何設計 Repository Pattern？
```

**Skill 會回應**：
```markdown
Repository Pattern 設計建議：

**策略選擇**：
- 簡單資料表導向：適合小型專案（< 10 表）
- 業務需求導向：適合中大型專案（推薦）

**範例**：訂單管理

✅ 推薦：OrderManagementRepository
   - 封裝完整業務邏輯（訂單 + 訂單明細）
   - 減少 Handler 複雜度

[提供完整程式碼範例]
```

---

## GitHub Copilot CLI 使用

### 安裝 GitHub CLI 與 Copilot 擴充功能

```bash
# 1. 安裝 GitHub CLI
winget install GitHub.cli

# 2. 登入 GitHub
gh auth login

# 3. 安裝 Copilot 擴充功能
gh extension install github/gh-copilot

# 4. 驗證安裝
gh copilot --version
```

### 使用 Agent

```bash
# 基本語法
gh copilot suggest -a dotnet-api "你的問題"

# 範例 1：實作新功能
gh copilot suggest -a dotnet-api "實作會員登入功能"

# 範例 2：程式碼審查
gh copilot suggest -a dotnet-api "審查這段 Handler 程式碼"

# 範例 3：架構諮詢
gh copilot suggest -a dotnet-api "訂單系統應該如何設計 Repository？"
```

### 使用 Skill

```bash
# 基本語法
gh copilot suggest -s api-template-bdd-guide "你的問題"

# 範例 1：BDD 測試
gh copilot suggest -s api-template-bdd-guide "如何撰寫會員註冊的 BDD 測試？"

# 範例 2：快取策略
gh copilot suggest -s api-template-bdd-guide "如何設計多層快取？"

# 範例 3：TraceContext
gh copilot suggest -s api-template-bdd-guide "TraceContext 如何在 Handler 中使用？"
```

---

## VS Code 使用

### 場景 1：開啟新專案時的互動式配置

1. **開啟專案資料夾**
   ```bash
   cd C:\Projects\MyNewAPI
   code .
   ```

2. **啟動 Copilot Chat**（`Ctrl + Shift + I`）

3. **輸入**：
   ```
   @dotnet-api 這是一個新專案，請協助我設定
   ```

4. **Agent 會自動檢測並引導**：
   ```markdown
   檢測到空白專案，開始互動式配置...
   
   1️⃣ 是否使用 GitHub 範本？
      a) 是（從 https://github.com/yaochangyu/api.template clone）
      b) 否（手動建立）
   ```

5. **繼續對話完成設定**

### 場景 2：實作功能時的步驟引導

**在 Chat 中輸入**：
```
@dotnet-api 我要實作會員註冊功能，選擇：
1. API First
2. 需要 Controller、Handler、Repository 三層
3. 需要 BDD 整合測試
```

**Agent 會提供完整步驟**：
```markdown
好的，我會協助你實作會員註冊功能。流程如下：

**步驟 1：定義 OpenAPI 規格**
請編輯 `doc/openapi.yml`，加入以下內容：
[提供 YAML 範例]

**步驟 2：產生 API 程式碼**
```bash
task codegen-api-server
task codegen-api-client
```

**步驟 3：撰寫 BDD 測試**
建立 `src/be/{Project}.IntegrationTest/Member/MemberRegistration.feature`
[提供 Gherkin 範例]

[後續步驟...]
```

### 場景 3：程式碼審查

1. **選取你的程式碼**（例如：MemberHandler.cs）

2. **右鍵 → Copilot → Review Code**

3. **或在 Chat 中輸入**：
   ```
   @dotnet-api 審查以下程式碼：
   [貼上程式碼]
   ```

**Agent 會指出問題並提供修正建議**

---

## 實際開發場景範例

### 範例 1：從零開始建立專案

```
User: @dotnet-api 我要建立一個新的求職平台 API 專案

Agent:
檢測到空白專案，讓我協助你設定。

1️⃣ 是否使用 GitHub 範本？
   a) 是（https://github.com/yaochangyu/api.template）
   b) 否

User: a

Agent:
好的，我會協助你 clone 範本。請執行：

```bash
git clone https://github.com/yaochangyu/api.template .
Remove-Item -Recurse -Force .git
```

接下來請回答以下問題：

2️⃣ 專案名稱？（例如：JobBank、EShop）

User: JobBank

Agent:
3️⃣ 資料庫選擇？
   a) SQL Server 2022（推薦）
   b) SQL Server 2019
   c) PostgreSQL

User: a

Agent:
4️⃣ 是否使用 Redis 快取？
   a) 是（推薦，L1+L2 混合快取）
   b) 否

User: a

Agent:
好的，我會協助你設定專案。請執行以下步驟...
[提供完整設定指令]
```

### 範例 2：實作會員註冊功能（完整流程）

```
User: @dotnet-api 實作會員註冊功能

Agent:
讓我確認幾個問題：
1. API First 還是 Code First？
2. 測試需求？

User: 
1. API First
2. 需要 BDD 整合測試

Agent:
**步驟 1：定義 OpenAPI 規格**

編輯 `doc/openapi.yml`：
```yaml
paths:
  /api/members:
    post:
      tags: [Members]
      summary: 註冊新會員
      operationId: CreateMember
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreateMemberRequest'
      responses:
        '201':
          description: 會員建立成功
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/MemberResponse'
        '400':
          description: 輸入驗證失敗
        '409':
          description: Email 已被使用

components:
  schemas:
    CreateMemberRequest:
      type: object
      required: [email, name, password]
      properties:
        email:
          type: string
          format: email
        name:
          type: string
          minLength: 2
          maxLength: 50
        password:
          type: string
          minLength: 8
```

**步驟 2：產生 API 程式碼**
```bash
task codegen-api-server
task codegen-api-client
```

**步驟 3：撰寫 BDD 測試**

建立 `src/be/JobBank.IntegrationTest/Member/MemberRegistration.feature`：
```gherkin
Feature: 會員註冊
  作為一個新使用者
  我想要註冊成為會員
  以便使用系統功能

  Scenario: 成功註冊新會員
    Given 我準備註冊會員資料
      | Email              | Name   | Password  |
      | test@example.com   | 測試員 | Pass@123  |
    When 我發送註冊請求至 "/api/members"
    Then 回應狀態碼應為 201
    And 資料庫中應存在此會員
```

**步驟 4：實作測試步驟**
[提供 MemberRegistrationSteps.cs 完整程式碼]

**步驟 5：實作 Handler**
[提供 MemberHandler.cs 完整程式碼]

**步驟 6：實作 Repository**
[提供 MemberRepository.cs 完整程式碼]

**步驟 7：實作 Controller**
[提供 MemberController.cs 完整程式碼]

**步驟 8：執行測試**
```bash
task test-integration
```

完成後你的會員註冊功能就實作完成了！
```

### 範例 3：程式碼審查與重構建議

```
User: @dotnet-api 審查這段程式碼

```csharp
public class MemberController : ControllerBase
{
    private readonly MemberRepository _repository;
    
    [HttpPost]
    public async Task<IActionResult> CreateMember(CreateMemberRequest request)
    {
        var exists = await _repository.ExistsByEmailAsync(request.Email);
        if (exists)
            return Conflict("Email 已被使用");
        
        var member = new Member
        {
            Email = request.Email,
            Name = request.Name
        };
        
        await _repository.CreateAsync(member);
        return Ok(member);
    }
}
```

Agent:
我發現以下問題：

❌ **問題 1：Controller 直接呼叫 Repository**
Controller 不應直接呼叫 Repository，應透過 Handler 層。

❌ **問題 2：業務邏輯在 Controller 中**
Email 重複檢查應在 Handler 處理。

❌ **問題 3：未使用 Result Pattern**
沒有統一的錯誤處理機制。

❌ **問題 4：回應狀態碼錯誤**
建立成功應回傳 201 Created，不是 200 OK。

✅ **建議修正**：

**1. 建立 Handler**
```csharp
public sealed class MemberHandler
{
    private readonly MemberRepository _repository;
    private readonly IValidator<CreateMemberRequest> _validator;
    
    public async Task<Result<MemberResponse>> CreateMemberAsync(
        CreateMemberRequest request,
        CancellationToken ct = default)
    {
        // 驗證
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<MemberResponse>(
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        
        // 重複檢查
        var exists = await _repository.ExistsByEmailAsync(request.Email, ct);
        if (exists)
            return Result.Failure<MemberResponse>("Email 已被使用", "DUPLICATE_EMAIL");
        
        // 建立會員
        var member = new Member
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = request.Email,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repository.CreateAsync(member, ct);
        
        return Result.Success(new MemberResponse
        {
            Id = member.Id,
            Email = member.Email,
            Name = member.Name,
            CreatedAt = member.CreatedAt
        });
    }
}
```

**2. 修正 Controller**
```csharp
public sealed class MemberController : MembersControllerBase
{
    private readonly MemberHandler _handler;
    
    public MemberController(MemberHandler handler)
    {
        _handler = handler;
    }
    
    public override async Task<IActionResult> CreateMember(
        CreateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _handler.CreateMemberAsync(request, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "DUPLICATE_EMAIL" => Conflict(new ErrorResponse 
                { 
                    Error = result.Error, 
                    Code = "DUPLICATE_EMAIL" 
                }),
                _ => BadRequest(new ErrorResponse { Error = result.Error })
            };
        }
        
        return CreatedAtAction(
            nameof(GetMember),
            new { id = result.Value.Id },
            result.Value);
    }
}
```

這樣修改後：
✅ 符合分層架構
✅ 使用 Result Pattern
✅ 正確的 HTTP 狀態碼
✅ 業務邏輯集中在 Handler
```

### 範例 4：快取策略諮詢

```
User: #api-template-bdd-guide 會員資料應該如何設計快取？

Skill:
會員資料快取策略建議：

**情境分析**：
- 讀取頻率：高（每次 API 呼叫都需驗證）
- 變動頻率：中（會員更新個人資料時）
- 資料大小：小（< 1KB）

**推薦策略**：L1 (5分鐘) + L2 (15分鐘)

**實作範例**：
```csharp
public async Task<Result<Member>> GetMemberAsync(string id, CancellationToken ct)
{
    var cacheKey = $"member:{id}";
    
    // 1. 嘗試從快取取得
    var cached = await _cache.GetAsync<Member>(cacheKey, ct);
    if (cached != null)
        return Result.Success(cached);
    
    // 2. 從資料庫查詢
    var member = await _repository.GetByIdAsync(id, ct);
    if (member == null)
        return Result.Failure<Member>("會員不存在", "NOT_FOUND");
    
    // 3. 寫入快取
    await _cache.SetAsync(cacheKey, member, TimeSpan.FromMinutes(15), ct);
    
    return Result.Success(member);
}
```

**更新時的快取失效**：
```csharp
public async Task<Result<Member>> UpdateMemberAsync(
    string id, 
    UpdateMemberRequest request, 
    CancellationToken ct)
{
    var member = await _repository.UpdateAsync(id, request, ct);
    
    // 清除快取
    await _cache.RemoveAsync($"member:{id}", ct);
    
    return Result.Success(member);
}
```

**注意事項**：
- ⚠️ 登入後的 Session 資料建議只存 L2 (Redis)
- ⚠️ 敏感資料（密碼雜湊）不要快取
- ⚠️ 更新會員資料時記得清除快取
```

---

## 🎯 常用指令快速參考

### VS Code Copilot Chat

| 功能 | 指令 |
|------|------|
| 使用 Agent | `@dotnet-api [問題]` |
| 使用 Skill | `#api-template-bdd-guide [問題]` |
| 審查程式碼 | `@dotnet-api 審查這段程式碼` |
| 實作功能 | `@dotnet-api 實作 [功能名稱]` |
| 架構諮詢 | `@dotnet-api [架構問題]` |

### GitHub CLI

| 功能 | 指令 |
|------|------|
| 使用 Agent | `gh copilot suggest -a dotnet-api "[問題]"` |
| 使用 Skill | `gh copilot suggest -s api-template-bdd-guide "[問題]"` |
| 解釋程式碼 | `gh copilot explain "[程式碼]"` |

---

## 💡 最佳實踐

1. **明確描述需求**：越詳細越好
   ```
   ❌ 不好：實作會員功能
   ✅ 好：實作會員註冊功能，使用 API First，需要 BDD 測試
   ```

2. **分階段互動**：複雜功能分多次對話
   ```
   第一次：確認架構與技術選型
   第二次：實作核心邏輯
   第三次：撰寫測試
   第四次：程式碼審查與優化
   ```

3. **提供上下文**：貼上相關程式碼
   ```
   @dotnet-api 我的 Repository 如下：
   [貼上程式碼]
   請建議如何優化
   ```

4. **利用檔案參考**：引用專案檔案
   ```
   @dotnet-api 參考 #file:MemberHandler.cs，
   實作類似的 OrderHandler
   ```

---

## 🚀 進階使用技巧

### 技巧 1：串連多個問題

```
@dotnet-api 
1. 實作訂單管理功能
2. 需要處理訂單與訂單明細的關聯
3. 使用 Repository Pattern 的需求導向設計
4. 需要 BDD 測試覆蓋以下情境：
   - 建立訂單（包含明細）
   - 查詢訂單（包含明細）
   - 更新訂單狀態
   - 取消訂單
```

### 技巧 2：結合 Skill 與 Agent

```
# 先用 Skill 查詢知識
#api-template-bdd-guide TraceContext 如何設計？

# 再用 Agent 實作
@dotnet-api 根據上述 TraceContext 設計，
實作在 OrderHandler 中記錄操作者資訊
```

### 技巧 3：程式碼片段審查

```
@dotnet-api 這段程式碼有沒有效能問題？

```csharp
public async Task<List<Order>> GetOrdersAsync()
{
    var orders = await _dbContext.Orders.ToListAsync();
    foreach (var order in orders)
    {
        order.Items = await _dbContext.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();
    }
    return orders;
}
```
```

---

## 📞 取得協助

如果 Agent 或 Skill 沒有正常運作：

1. **確認檔案存在**：
   ```bash
   dir .github\dotnet-contribution\agents
   dir .github\dotnet-contribution\skills
   ```

2. **重新載入 VS Code**：
   - `Ctrl + Shift + P` → "Developer: Reload Window"

3. **檢查 Copilot 狀態**：
   - `Ctrl + Shift + P` → "GitHub Copilot: Check Status"

4. **查看日誌**：
   - `Ctrl + Shift + P` → "Developer: Show Logs" → "Extension Host"

---

*最後更新時間：2026-01-01*
