# 🚀 快速開始指南

5 分鐘學會使用 `dotnet-api` Agent！

## ⚡ 最快上手方式（VS Code）

### 步驟 1：開啟專案

```bash
cd your-project-folder
code .
```

### 步驟 2：開啟 Copilot Chat

按下 `Ctrl + Shift + I`（Windows）或 `Cmd + Shift + I`（Mac）

### 步驟 3：輸入指令

在 Chat 視窗中輸入：

```
@dotnet-api 我要實作會員註冊功能
```

### 步驟 4：跟隨互動式問答

Agent 會詢問你：
- API 開發方式（API First / Code First）
- 需要實作的層級（Controller / Handler / Repository）
- 測試需求（BDD / 單元測試 / 無）

### 步驟 5：取得完整實作指引

Agent 會提供：
- ✅ 分步驟的實作指引
- ✅ 完整的程式碼範例
- ✅ BDD 測試範本
- ✅ 開發指令

---

## 💡 三個最常用的場景

### 1️⃣ 實作新功能

```
@dotnet-api 實作 [功能名稱]
```

**範例**：
- `@dotnet-api 實作會員登入功能`
- `@dotnet-api 實作訂單查詢 API`
- `@dotnet-api 實作產品分類管理`

### 2️⃣ 程式碼審查

選取程式碼後，在 Chat 輸入：

```
@dotnet-api 審查這段程式碼
```

或右鍵 → **Copilot** → **Review Code**

### 3️⃣ 架構諮詢

```
@dotnet-api [架構問題]
```

**範例**：
- `@dotnet-api 訂單系統應該如何設計 Repository？`
- `@dotnet-api 會員資料應該如何設計快取策略？`
- `@dotnet-api 如何實作 TraceContext 追蹤？`

---

## 📖 查詢知識庫

使用 Skill 查詢特定主題：

```
#api-template-bdd-guide [問題]
```

**範例**：
- `#api-template-bdd-guide 如何撰寫 BDD 測試？`
- `#api-template-bdd-guide TraceContext 如何使用？`
- `#api-template-bdd-guide 多層快取策略是什麼？`

---

## 🎯 完整範例：實作會員註冊（端到端）

### 對話紀錄

```
👤 User:
@dotnet-api 實作會員註冊功能

🤖 Agent:
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

---

👤 User:
1. API First
2. 需要全部三層
3. 僅 BDD 整合測試

---

🤖 Agent:
好的！我會協助你使用 API First 方式實作會員註冊功能（包含 BDD 測試）。

**步驟 1：編輯 OpenAPI 規格**

開啟 `doc/openapi.yml`，加入以下內容：

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
    
    MemberResponse:
      type: object
      properties:
        id:
          type: string
        email:
          type: string
        name:
          type: string
        createdAt:
          type: string
          format: date-time
```

**步驟 2：產生 API 程式碼**

```bash
task codegen-api-server
task codegen-api-client
```

**步驟 3：撰寫 BDD 測試**

建立檔案：`src/be/JobBank.IntegrationTest/Member/MemberRegistration.feature`

[提供完整 .feature 檔案內容]

**步驟 4：實作測試步驟**

建立檔案：`src/be/JobBank.IntegrationTest/Member/MemberRegistrationSteps.cs`

[提供完整測試步驟程式碼]

**步驟 5：實作 Handler**

建立檔案：`src/be/JobBank.WebAPI/Member/MemberHandler.cs`

[提供完整 Handler 程式碼]

**步驟 6：實作 Repository**

建立檔案：`src/be/JobBank.WebAPI/Member/MemberRepository.cs`

[提供完整 Repository 程式碼]

**步驟 7：實作 Controller**

建立檔案：`src/be/JobBank.WebAPI/Member/MemberController.cs`

[提供完整 Controller 程式碼]

**步驟 8：註冊服務（Program.cs）**

在 `Program.cs` 中加入：

```csharp
// 註冊 Handler 與 Repository
builder.Services.AddScoped<MemberHandler>();
builder.Services.AddScoped<MemberRepository>();

// 註冊驗證器
builder.Services.AddScoped<IValidator<CreateMemberRequest>, CreateMemberRequestValidator>();
```

**步驟 9：執行測試**

```bash
task test-integration
```

如果測試通過，表示會員註冊功能實作完成！🎉

---

需要我詳細解釋任何步驟嗎？
```

---

## 📋 常用指令速查表

| 用途 | 指令 |
|------|------|
| 實作新功能 | `@dotnet-api 實作 [功能]` |
| 程式碼審查 | `@dotnet-api 審查這段程式碼` |
| 架構諮詢 | `@dotnet-api [架構問題]` |
| 查詢知識 | `#api-template-bdd-guide [主題]` |
| BDD 測試 | `#api-template-bdd-guide 如何撰寫 BDD 測試？` |
| 快取策略 | `#api-template-bdd-guide 快取策略建議` |
| TraceContext | `#api-template-bdd-guide TraceContext 使用方式` |

---

## ⚙️ 疑難排解

### 問題：Agent 沒有回應

**解決方式**:
1. 確認檔案存在：`.github\dotnet-contribution\agents\dotnet-api.md`
2. 重新載入 VS Code：`Ctrl + Shift + P` → "Developer: Reload Window"
3. 檢查 Copilot 狀態：`Ctrl + Shift + P` → "GitHub Copilot: Check Status"

### 問題：回應不符合預期

**解決方式**：
1. **更明確的描述**：提供更多上下文
2. **分階段提問**：複雜問題拆分成多次對話
3. **提供範例**：貼上相關程式碼或檔案

### 問題：不知道該問什麼

**建議起手式**：
```
@dotnet-api 這個專案的開發流程是什麼？
```

或

```
#api-template-bdd-guide 這個專案的架構是什麼？
```

---

## 📚 更多資源

- **完整使用範例**：[USAGE_EXAMPLES.md](./USAGE_EXAMPLES.md)
- **檔案參考說明**：[FILE_REFERENCES.md](./FILE_REFERENCES.md)
- **專案開發指南**：[../../CLAUDE.md](../../CLAUDE.md)

---

**祝你開發順利！** 🚀

有任何問題，隨時問 `@dotnet-api`！
