# CLAUDE.md

此檔案為 Claude Code (claude.ai/code) 在此專案中工作時的指導文件。  
使用台灣用語的繁體中文回覆。

**📝 核心原則**: 詳細實作指南已移到 SKILL 系統（見末尾索引）。本檔案著重於 AI 助理的行為規則。

## 文檔導航（三層架構）

- **此檔案** — AI 助理行為規則（強制互動、偵測機制、計畫管理）
- [development-rules.md](./.claude/development-rules.md) — 開發規則（所有人需要）
- [decision-framework.md](./.claude/decision-framework.md) — 決策邏輯（特定工作流需要）

---

## AI 助理使用規則

### 核心互動原則（強制遵守）

1. **強制互動確認**
   - **必須** 使用 `AskUserQuestion` 工具進行結構化詢問（Claude CLI）
   - 在所有需要使用者決策的情境下，都必須明確詢問，不得擅自執行
   - 提供清晰的選項說明，幫助使用者做出明智選擇

   **API 開發流程決策（強制確認）**
   
   開發任何新的 API 端點前，必須詢問使用者確認開發方式（API First 或 Code First）。
   - **決策依據和詳細對比**，參考 [decision-framework.md 的「## API 開發流程決策」章節](./decision-framework.md#api-開發流程決策)
   - **執行流程參考**：[/api-development SKILL](./skills/api-development/SKILL.md)
   - ⚠️ **禁止混用**：同一專案內不得混用 API First 和 Code First

2. **不得擅自假設**
   - 即使文件標註「預設」值，仍須詢問使用者確認
   - 例外：使用者已在對話中明確指定（如「使用 SQL Server」）

3. **分階段互動**
   - 單次詢問最多 3-4 個問題，避免資訊過載
   - 複雜流程應分階段進行

4. **完整性優先**
   - 必須收集所有必要資訊後才開始執行

### 專案狀態檢測機制

首次接觸此專案時，檢測以下條件：
- ✅ 存在 `dotnet-project-template/env/.template-config.json`
- ✅ 存在 `.sln` 解決方案檔案與 `src/` 目錄
- ✅ 存在 `appsettings.json` 與 `docker-compose.yml`

若檢測失敗（空白專案），詢問是否使用 GitHub 範本 `https://github.com/yaochangyu/api.template`。

---


## 進階主題索引

當實作具體功能時，根據需求查閱相應 SKILL（詳細實作指南已分層組織）：

### 必讀文檔
1. [development-rules.md](./.claude/development-rules.md) — 強制詢問情境、禁止行為、最佳實踐
2. [decision-framework.md](./.claude/decision-framework.md) — API 流程、測試策略、快取設計決策

### SKILL 索引表

| 工作項 | 相關 SKILL | 前置閱讀 |
|--------|-----------|---------|
| 設計 API 端點 | `/api-development` | decision-framework |
| 實作 Controller | `/handler` | development-rules |
| 設計資料存取 | `/repository-design` | decision-framework |
| 處理錯誤 | `/error-handling` | development-rules |
| 實作中介軟體 | `/middleware` | development-rules |
| EF Core 最佳化 | `/ef-core` | development-rules |
| 快取設計 | `/caching-strategy` | decision-framework |
| 測試實作 | `/bdd-testing` | development-rules + decision-framework |
| 專案初始化 | `/project-init` | decision-framework |
| 安全性檢查 | `/security-check-*` | development-rules |

---

## 計畫書生命週期管理

當建立新計畫書時，遵循以下流程：

### 建立階段
- 檔案名格式: `{project}-plan.md` 或 `{project}-PLAN.md`
- 位置: 專案根目錄（方便檢視）
- 開頭標記: `**計畫時間**: YYYY-MM-DD HH:mm GMT+8` 與 `**進度**: [計畫中]`

### 執行階段
- 即時更新進度追蹤表
- 每完成一個 Step，標記狀態為 ✅ 與進度百分比

### 封存階段（計畫全部完成後）
1. **移到 archive 目錄**: `{project}-plan.md` 搬到 .archive/
2. **在檔案頂部加標記**:
   ```markdown
   ⏹️ **Status**: COMPLETED (YYYY-MM-DD HH:mm GMT+8)
   
   此計畫已全部執行完成，已封存至 .archive/
   詳見 Git commit: {commit-sha}
   ```
3. **Git commit** — commit message 中**不包含 Co-authored-by**（遵守全域規則）


---

**文件版本**: 4.0（AI 助理規則，2026-07-10 重構）  
**最後更新**: 2026-07-10

