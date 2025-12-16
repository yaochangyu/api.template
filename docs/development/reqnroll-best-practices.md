# Reqnroll 最佳實踐（Context7 摘要 + 團隊落地）

本文彙整 Reqnroll（.NET BDD）官方重點並結合本庫用法，提供可落地的規範、反模式與配置建議。專案中現有範例可參考：
- Feature 範例：[_01_Demo/飯粒.feature](../../src/be/JobBank1111.Job.IntegrationTest/_01_Demo/飯粒.feature)
- Step 範例：[_01_Demo/飯粒Step.cs](../../src/be/JobBank1111.Job.IntegrationTest/_01_Demo/飯粒Step.cs)
- 共用情境輔助： [ScenarioContextExtension.cs](../../src/be/JobBank1111.Job.IntegrationTest/ScenarioContextExtension.cs)

## 目標
- 以 Gherkin（Feature/Scenario）撰寫可執行規格，並用 Step 綁定到可重用的測試邏輯。
- 在 CI 中穩定、快速地執行整合/端點層測試；失敗訊息具可觀察性。
- 減少重複 Step、提高維護性（資料工廠、共用擴充、參數轉換）。

## 安裝與測試框架
- 套件選擇：Reqnroll 支援 NUnit、MsTest、xUnit。若無特別偏好，官方建議 NUnit；本庫採用 xUnit（參見專案相依 `Reqnroll.xUnit`）。
- xUnit 限制：`Console.WriteLine` 不會出現在輸出；建議注入 `ITestOutputHelper` 記錄除錯訊息（官方範例）。
- NuGet（依框架選擇）：
  - `Reqnroll.NUnit` 或 `Reqnroll.MsTest` 或 `Reqnroll.xUnit`
  - 測試執行必備：`Microsoft.NET.Test.Sdk`、相對應 runner/adapter

## 專案結構建議
- `*.feature` 與對應 Step 類別同層或鄰近模組，易於追蹤與維護。
- 以子資料夾劃分領域或場景群組，例如 `_01_Demo`、`Members` 等。
- 將共用基礎設施（HttpClient、DB 工廠、假外部服務、Header/Query/Body 組裝、驗證工具）封裝於擴充類別或測試共用程式庫（本庫：`ScenarioContextExtension`）。

## Gherkin 規格規範
- 命名：`Feature` 以業務能力命名，`Scenario` 以使用者價值 + 關鍵條件命名。
- 結構：
  - `Background` 放共同前置（如 Header、Query、Init Server）。
  - 多組資料請使用 `Scenario Outline + Examples`，避免重複 Scenario。
- 語意：每步驟做一件事；`Then` 驗證可觀察輸出（HTTP 狀態碼、Header、Body），不要驗證內部 ORM 細節。
- 範例：見 [飯粒.feature](../../src/be/JobBank1111.Job.IntegrationTest/_01_Demo/飯粒.feature)。

## Step 定義（Bindings）
- 標記：Step 類別必須加上 `[Binding]`（官方文件）。
- 表達式：優先使用 Cucumber Expressions（如 `{string}`, `{int}`），必要時才用 Regex。
- 非同步：I/O 步驟（HTTP/DB）使用 `async/await`（官方文件：Asynchronous Bindings）。
- 範圍（Scoped Bindings）：用 `[Scope(Tag=.../Feature=.../Scenario=...)]` 控制同名步驟在特定情境才生效，避免衝突（官方文件）。
- 資料表/多行字串：
  - DataTable 以 `DataTable` 參數接收，或使用 Assist 轉型為強型別集合（官方 Step Argument Transformations）。
  - DocString 對應 `string` 參數，適合 JSON 片段（本庫已示範）。
- 範例：見 [飯粒Step.cs](../../src/be/JobBank1111.Job.IntegrationTest/_01_Demo/飯粒Step.cs)。

## Context 與 DI
- 取得情境：於建構子或方法參數注入 `ScenarioContext`/`FeatureContext`（官方：不要使用過時的靜態 `ScenarioContext.Current`）。
- 共用資料：集中封裝於 Extension（本庫：`ScenarioContextExtension`）以降低重複與易讀性。
- xUnit 輸出：將 `Xunit.Abstractions.ITestOutputHelper` 注入 Binding 類別以輸出測試日誌（官方 xUnit 整合）。

## Hooks（Before/After）
- 生命週期：`[BeforeScenario]`、`[AfterScenario]` 等用於環境初始化/清理。
- 順序：以 `Order` 屬性控制執行先後（官方文件）。
- 依 Tag 套用：`[BeforeScenario("@tag")]` 僅在特定情境執行。
- 例外處理：`Before` 丟例外會阻斷後續 `Before`，但 `After` 仍嘗試執行，清理邏輯需判斷初始化是否成功（官方文件）。

## 平行執行（Parallel Execution）
- 基本原則：避免使用任何跨測試的靜態共享狀態；所有情境依賴透過 Context/DI 提供（官方文件）。
- xUnit：可利用 Reqnroll xUnit 插件提供的 `ReqnrollNonParallelizableFeatures` 集合，或以標籤與產碼設定將特定 Feature 標為不可平行（官方文件）。
- 資料隔離：
  - DB：使用每測試唯一 Schema/資料前綴或 Testcontainers 啟動隔離的 DB。
  - HTTP 假端點：每個情境產生獨立路徑或在 Background 重新註冊。

## 設定（reqnroll.json）
- 檔名：`reqnroll.json`（取代 `specflow.json`，官方文件）。
- 常用設定：
  - `bindingAssemblies`：註冊外部步驟組件（例如共用步驟庫）。
  - `formatters`：產出報表（HTML、Cucumber Messages）以利活文件與工具整合。
  - `trace.stepDefinitionSkeletonStyle`：需要 Regex Skeleton 時可調整。
- 範例（HTML 與 Messages 報表）：

```json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "formatters": {
    "html":    { "outputFilePath": "report/living_doc.html" },
    "message": { "outputFilePath": "report/cucumber_messages.ndjson" }
  }
}
```

## 驗證與比對
- JSON 驗證：
  - 小規模直接比對字串（DocString）；較大結構可用 JSON Path 局部斷言；或採快照工具（Verify）整合（官方 Verify 範例）。
- HTTP 回應：驗證狀態碼、Header（分頁、中繼資訊）、Body（結構與關鍵欄位）。
- DB 驗證：Given 建置資料，Then 以查詢比對關鍵欄位與筆數；避免耦合實作者件。

## 標籤（Tags）約定
- `@wip`：僅在本機執行或 CI 排除。
- `@manual`：標註手動情境；可透過 Scoped Bindings 使其顯示於追蹤但不執行（官方範例）。
- `@non-parallel`：需要序列化的情境/功能；可搭配產碼設定使其不可平行（官方文件）。

## 反模式（避免）
- 在 Then 進行動作（造成隱藏副作用）。
- 於 Step 中直接 new HttpClient/DbContext、重複建構；請由 Hook/DI 建立並置於 Context。
- 大量相似字面步驟導致維護困難；改採參數化步驟或 Scoped Bindings。
- 使用 `ScenarioContext.Current`/`FeatureContext.Current`（已不支援且與平行衝突）。

## CI 與輸出
- 測試分片與平行：以 Feature/資料夾維度切分；確保資料與假端點隔離。
- 報表：啟用 `formatters` 產出 HTML 與 Cucumber Messages，作為活文件與失敗排查依據。
- 輸出可觀察性：失敗訊息包含 traceId、請求摘要、Diff/Path 錯誤點。

## 快速檢核（Checklist）
- Feature/Scenario 以使用者價值命名，背景共用寫在 Background。
- Step 使用 Cucumber Expressions；I/O 步驟為 async；避免重複步驟語句。
- 依賴（HttpClient/Db/外部服務）皆由 Hook/DI/Context 提供，無跨情境靜態狀態。
- DataTable 轉型採 Assist 或強型別模型；DocString 用於 JSON 片段。
- 平行執行安全；必要者以標籤與集合/設定序列化。
- 啟用 `reqnroll.json` 報表；CI 產出 HTML/NDJSON 成果並歸檔。

---
參考來源（Context7 摘要）
- xUnit 整合與輸出：官方 `integrations/xunit.md`
- 平行執行與情境存取：官方 `execution/parallel-execution.md`
- Bindings 與 Step 定義：官方 `automation/bindings.md`、`automation/step-definitions.md`
- 非同步步驟：官方 `automation/asynchronous-bindings.md`
- Scoped Bindings：官方 `automation/scoped-bindings.md`
- Hooks：官方 `automation/hooks.md`
- 參數轉換/Assist：官方 `automation/step-argument-conversions.md`
- 設定檔：官方 `installation/configuration.md`、`reporting/reqnroll-formatters.md`
- Quickstart：官方 `docs/quickstart/index.md`