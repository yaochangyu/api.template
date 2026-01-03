# 專案初始化 Skill (`project-init`)

## 職責
此 Skill 負責引導使用者完成專案的初始化設定，包括：
1. 檢測當前專案狀態，判斷是否為「空白專案」。
2. 根據檢測結果，與使用者互動，詢問是否使用 GitHub 範本。
3. 若使用者同意，執行範本的下載與初始化操作（例如 `git clone`，並移除 `.git` 目錄）。
4. 依據使用者的選擇，設定初始專案配置並儲存至 `env/.template-config.json`。

## 核心流程
本 Skill 的執行依據 `CLAUDE.md` 中「AI 助理使用規則」章節的「專案狀態檢測機制」流程圖與「GitHub 範本套用規則」進行。

## 互動原則
所有使用者互動將嚴格遵循 `references/interaction-rules.md` 中定義的「核心互動原則」與「強制詢問情境」。

### 檢測條件 (引用自 `references/interaction-rules.md`)
- 不存在 `env/.template-config.json` 配置檔案
- 不存在 `.sln` 解決方案檔案
- 不存在 `src/` 目錄或該目錄為空
- 不存在 `appsettings.json` 或 `docker-compose.yml`

### GitHub 範本套用規則 (引用自 `references/interaction-rules.md`)
1. **安全檢查（不得擅自覆蓋）**：僅在「工作目錄為空」或使用者已明確同意覆蓋/清空時，才可執行 clone。若工作目錄非空，必須先詢問使用者要「改用子資料夾」或「取消」。
2. **使用 git clone 下載範本**：例如：`git clone https://github.com/yaochangyu/api.template .`
3. **刪除 Git 相關資料**：刪除 `.git/` 目錄。例如：`Remove-Item -Recurse -Force .git`
4. **接著才進入本專案的互動式配置**。

## 輸出
- 成功初始化的專案結構。
- `env/.template-config.json` 配置檔案，記錄專案設定。

## 範例互動流程 (Agent 調用此 Skill 時)
1. **Agent**: 偵測到專案為空白，將詢問使用者是否初始化。
2. **Agent**: "檢測到此專案可能為空白專案。您是否希望使用 GitHub 範本 `https://github.com/yaochangyu/api.template` 進行初始化？"
   - 選項: [是] [否] [取消]
3. **使用者**: 選擇「是」。
4. **Agent**: "當前工作目錄非空。請問您希望：1) 在子資料夾中初始化範本；2) 清空目前目錄並初始化範本；3) 取消操作？"
5. **使用者**: 選擇「2) 清空目前目錄並初始化範本」。
6. **Skill 執行**: 執行 `git clone`，刪除 `.git`，然後進入配置詢問。
7. **Agent**: "請選擇資料庫類型：1) SQL Server；2) PostgreSQL；3) MySQL。"
8. ... (後續配置流程)
