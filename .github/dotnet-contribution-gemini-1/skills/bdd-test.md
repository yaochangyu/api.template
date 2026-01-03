# BDD 測試 Skill (`bdd-test`)

## 職責
此 Skill 負責引導使用者進行行為驅動開發 (BDD) 的測試實作，確保功能符合需求規格。

## 核心流程
本 Skill 遵循 `CLAUDE.md` 中「BDD 開發流程」的指導，並整合「測試策略詢問」的互動機制。

### 1. 測試需求與範圍詢問 (強制互動)
本 Skill 會首先強制詢問使用者關於測試的需求與範圍，包括：
- 是否需要實作測試？ (BDD 整合測試、單元測試或兩者)
- 測試範圍為何？ (完整測試、核心業務邏輯、關鍵路徑、異常情境)
- BDD 測試情境細節？ (是否已有 `.feature` 檔案，需要新增哪些情境，是否需要 AI 協助撰寫 Gherkin 語法)
- 測試資料準備策略？ (Docker 容器、固定資料、動態產生、資料清理)
- 測試方法選擇？ (強調 API 端點必須使用 BDD，優先使用 Testcontainers)

### 2. Gherkin 情境撰寫
- 根據使用者的輸入或需求，引導使用者撰寫 `.feature` 檔案中的 Gherkin 語法。
- 提供 `assets/bdd-feature-template.feature` 作為範本。

### 3. 測試步驟實作
- 引導使用者為 Gherkin 情境實作 C# 測試步驟。
- 強調使用 `TestServer` 和 `ScenarioContext` 進行整合測試，並優先使用 Docker 容器作為測試替身。
- 提供 `assets/bdd-steps-template.cs` 作為範本。

### 4. 測試執行指導
- 指導使用者如何執行 BDD 整合測試（例如 `task test-integration`）。

## 引用文件
- `references/interaction-rules.md`: 用於所有使用者互動的規範。
- `references/bdd-workflow.md`: 詳細說明 BDD 開發循環、Docker 優先測試策略、API 控制器測試指引。
- `assets/bdd-feature-template.feature`: Gherkin 語法範本。
- `assets/bdd-steps-template.cs`: C# BDD 測試步驟實作範本。

## 輸出
- `.feature` 檔案 (Gherkin 情境)。
- 測試步驟實作程式碼 (`.cs` 檔案)。
- 指導使用者執行和驗證測試。

## 範例互動流程 (Agent 調用此 Skill 時)
1. **Agent**: "您好！我們將討論新功能的測試策略。首先，請問是否需要實作測試？\n1) 是，需要實作完整測試（BDD 整合測試 + 單元測試）\n2) 是，僅需要 BDD 整合測試\n3) 是，僅需要單元測試\n4) 否，暫不實作測試"
2. **使用者**: 選擇 "2"。
3. **Agent**: "好的，僅 BDD 整合測試。請問測試範圍為何？\n1) 新增功能的完整測試\n2) 僅測試核心業務邏輯\n3) 僅測試關鍵路徑 (Happy Path)\n4) 包含異常情境與邊界條件"
4. **使用者**: 選擇 "1,4"。
5. **Agent**: "請提供您希望測試的 Gherkin 情境描述 (Given-When-Then 格式)，或讓 AI 協助撰寫。"
6. ... (使用者提供情境後，Agent 產生 `.feature` 檔案，並引導實作測試步驟)
