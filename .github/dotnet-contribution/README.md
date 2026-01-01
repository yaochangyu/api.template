# GitHub Copilot Extension for API Template

本專案的 GitHub Copilot 擴充功能，提供針對此 API Template 最佳化的 AI 助手與技能。

## 📦 內容

### Agents（AI 助手）

| Agent | 模型 | 描述 |
|-------|------|------|
| **api-template-architect** | Sonnet | 專精於此 API Template 的 .NET 架構師，熟悉 BDD 測試流程、分層架構設計、TraceContext 模式、Redis 快取策略、OpenAPI 程式碼產生 |

### Skills（技能包）

| Skill | 描述 |
|-------|------|
| **api-template-bdd-guide** | ASP.NET Core 8.0 Web API 開發指南，涵蓋分層架構（Controller-Handler-Repository）、BDD 測試流程（Reqnroll）、Result Pattern、Redis 快取策略、TraceContext 中介軟體設計 |

### 資源檔案

**範本檔案 (assets/)**
- `handler-template.cs` - Handler 實作範本（Result Pattern + 快取）
- `repository-template.cs` - Repository 實作範本（EF Core + Dapper）
- `bdd-feature-template.feature` - BDD 情境範本（Gherkin）
- `bdd-steps-template.cs` - BDD 測試步驟範本（Reqnroll）

**參考文件 (references/)**
- `trace-context-design.md` - TraceContext 設計說明
- `cache-strategy.md` - 多層快取策略詳解
- `openapi-codegen.md` - OpenAPI 程式碼產生工作流程

## 🚀 使用方式

### GitHub Copilot Chat

在 VS Code 或 Visual Studio 中使用 GitHub Copilot Chat：

```
@api-template-architect 我要實作會員註冊功能
```

```
#api-template-bdd-guide 如何撰寫 BDD 測試？
```

### GitHub Copilot CLI（終端機）

```bash
# 使用 agent
gh copilot suggest -a api-template-architect "實作會員登入功能"

# 使用 skill
gh copilot suggest -s api-template-bdd-guide "如何設計 Repository？"
```

### Claude Code CLI

```bash
# 啟動互動式對話
claude -p "Act as api-template-architect and help me implement member registration"

# 使用 skill
claude -p "Use api-template-bdd-guide skill to design a cache strategy"
```

## 📖 核心功能

### 1. 互動式專案初始化

Agent 會在首次接觸專案時，自動檢測專案狀態並引導設定：

```
檢測到空白專案，開始互動式配置...

1️⃣ 是否使用 GitHub 範本？
   a) 是（從 https://github.com/yaochangyu/api.template clone）
   b) 否（手動建立）

2️⃣ 專案名稱？（例如：JobBank、EShop）

3️⃣ 資料庫選擇？
   a) SQL Server 2022（推薦）
   b) SQL Server 2019
   c) PostgreSQL

... (更多設定)
```

### 2. API First 開發指導

Agent 會引導你使用 OpenAPI 規格驅動開發：

1. 編輯 `doc/openapi.yml`
2. 執行 `task codegen-api-server` 產生 Controller
3. 執行 `task codegen-api-client` 產生客戶端
4. 實作 Handler 與 Repository
5. 撰寫 BDD 測試

### 3. BDD 測試輔助

提供完整的 BDD 測試範本與步驟實作：

```gherkin
Feature: 會員註冊
  作為一個新使用者
  我想要註冊成為會員
  以便使用系統功能

  Scenario: 成功註冊新會員
    Given 我準備註冊會員資料
    When 我發送註冊請求
    Then 回應狀態碼應為 201
    And 資料庫中應存在此會員
```

### 4. 程式碼審查標準

自動檢查：
- ✅ 分層職責是否正確
- ✅ 非同步模式是否正確
- ✅ 快取策略是否合理
- ✅ EF Core 效能最佳化
- ✅ 錯誤處理是否使用 Result Pattern

## 💡 使用範例

### 範例 1：實作新功能

```
User: @api-template-architect 我要實作會員登入功能

Agent:
我會協助你實作會員登入功能。首先讓我確認幾個問題：

**1. API 開發方式**
1️⃣ API First（推薦）：先定義 OpenAPI 規格
2️⃣ Code First：直接實作程式碼

**2. 測試需求**
1️⃣ 完整測試（BDD + 單元測試）
2️⃣ 僅 BDD 整合測試
3️⃣ 暫不實作測試

請告訴我你的選擇。
```

### 範例 2：程式碼審查

```
User: @api-template-architect 幫我審查這段程式碼

Agent:
我發現以下問題：

❌ **問題 1：Controller 中有業務邏輯**
業務邏輯應移至 Handler，Controller 只處理 HTTP 關注點。

✅ **建議修正**：
[提供修正後的程式碼範例]
```

### 範例 3：架構諮詢

```
User: #api-template-bdd-guide 訂單與訂單明細應該如何設計 Repository？

Agent:
建議採用**需求導向的 Repository 設計**：

✅ OrderManagementRepository
   - CreateOrderWithItemsAsync() - 建立訂單（包含明細）
   - GetOrderWithItemsAsync() - 取得訂單（包含明細）

優點：封裝完整業務邏輯、減少跨層呼叫

[提供完整程式碼範例]
```

## 🎯 適用情境

- ✅ 使用此 API Template 建立新專案
- ✅ 實作基於 BDD 的整合測試
- ✅ 設計分層架構（Controller-Handler-Repository）
- ✅ 配置 Redis 多層快取策略
- ✅ 實作 TraceContext 追蹤機制
- ✅ 從 OpenAPI 規格產生 API 程式碼
- ✅ 程式碼審查與架構決策

## 📚 延伸閱讀

- [CLAUDE.md](../../CLAUDE.md) - 完整的專案開發指南
- [API Template Repository](https://github.com/yaochangyu/api.template) - GitHub 專案範本

## 🤝 貢獻

歡迎提交 Issue 或 Pull Request 改善此擴充功能。

## 📄 授權

MIT License
