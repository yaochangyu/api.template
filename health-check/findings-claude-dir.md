# findings-claude-dir.md — .claude/ 目錄一致性健檢

- 掃描者：scan-claude-dir（Explore agent，唯讀）；本檔由主對話代為落檔
- 掃描範圍：`.claude/`（agents / skills / commands / templates / settings），對照 `.github/` 殘留與實際專案結構
- 環境提醒：本機 `/mnt/d`（Windows drvfs）為**大小寫不敏感**檔案系統，會遮蔽大小寫相關問題（見發現 1）
- 發現總數：12（blocker 1、major 5、minor 6）

## 資源清單
- **.claude/agents/（4）**：architecture-review、feature-development、project-setup、testing-strategy
- **.claude/commands/（2）**：jb.md（/jb codegen/專案管理）、webapi.md（/webapi codegen 系列）
- **.claude/templates/（5 .cs）**：controller / handler / middleware / model / repository 範本，供 command-processor 使用
- **.claude/skills/（17 skill）**：api-development、bdd-practices、bdd-testing、ef-core、error-handling、handler、middleware、project-init、repository-design、skill-agent-design、skill-creator、webapi-real-testing、security-check-config/-dependencies/-secrets、security-deep-review、security-fast-scan；另有 templates/（非 skill）與 STRUCTURE-REPORT.md
- **其他根層**：README.md、agents-workflow-guide.md、command-processor.md、settings.local.json

## 發現詳情

### [BLOCKER] 1. skill 主檔大小寫不一致，Linux 上會靜默失效
- 小寫 `skill.md`（9）：api-development、bdd-testing、ef-core、error-handling、handler、middleware、project-init、repository-design、skill-agent-design
- 大寫 `SKILL.md`（8）：bdd-practices、skill-creator、webapi-real-testing、security-check-config/-dependencies/-secrets、security-deep-review、security-fast-scan
- 問題：Claude Code 規格要求 `SKILL.md`。本機 drvfs 大小寫不敏感故仍可載入、問題被遮蔽；此 repo 是散佈範本，clone 到大小寫敏感的 Linux/CI 後小寫者會找不到主檔而靜默失效。
- 建議：全部統一為 `SKILL.md`，以 `git mv` 提交（純大小寫改名需 git mv 才會被記錄）。
- 不確定性：Claude Code 是否對小寫有相容 fallback，無法從 repo 內確認。

### [MAJOR] 2. STRUCTURE-REPORT.md 過期且指引方向相反
- 位置：`.claude/skills/STRUCTURE-REPORT.md`
- 問題：(a) 主張「檔名必須小寫 skill.md」（依據 GitHub Copilot 文件），與 Claude Code 的 SKILL.md 要求相反；(b) 宣稱已把三個檔案改小寫，與現況不符（那三個仍為大寫）；(c) 以 `.github/skills` 為題，遷入 `.claude/` 後失去脈絡。
- 建議：刪除，或改寫為以 Claude Code SKILL.md 規格為準。

### [MAJOR] 3. 多個 skill/agent 仍以 GitHub Copilot 為對象並引用 .github/ 路徑
- 位置：skill-agent-design/skill.md（多處 `.github/skills/`、`.github/agents/`、`.agent.md`）、skill-creator/SKILL.md（同類）；另含 Copilot 字樣：api-development、handler、project-init（含 references）、repository-design references、security-fast-scan、webapi-real-testing。
- 問題：遷入 `.claude/` 後平台假設與路徑與 Claude Code 實際結構不符；作為「建立 skill/agent」教學會產出錯誤位置的檔案。
- 建議：改寫為 Claude Code 版路徑（`.claude/skills/<name>/SKILL.md`、`.claude/agents/<name>.md`）。

### [MAJOR] 4. .cs 範本存在兩份分歧副本（雙來源）
- 位置：`.claude/templates/*.cs` vs `.claude/skills/*/assets/*.cs`
- 事實：controller-template.cs 兩處內容**不同**（diff 有差異）；handler、repository 亦於兩處各有版本（skills 側 repository 另分 domain/table 兩版）。
- 問題：command（讀 .claude/templates/）與 skill（讀各自 assets/）會產出不一致的程式碼。
- 建議：確立單一來源，另一處改為引用或移除。

### [MAJOR] 5. api-development 參考文件含失效相對連結
- 位置：`.claude/skills/api-development/references/api-development-workflow.md:678`
- 問題：`[本專案 OpenAPI 規格](../../../doc/openapi.yml)` 解析為 `.claude/doc/openapi.yml`，不存在（實檔在 dotnet-project-template/doc/openapi.yml）。

### [MAJOR] 6. 大量 doc/openapi.yml 引用與實際結構不符
- 位置：`.claude/README.md:204`、api-development/skill.md（13、26、115、119、123、293、373）、assets/openapi-endpoint-template.yml:4、references/api-development-workflow.md 多處
- 問題：假設 openapi 位於 repo 根層 `doc/openapi.yml`，但實檔在 `dotnet-project-template/doc/openapi.yml`；AI 依字面操作會找不到檔。
- 建議：標註「路徑相對於套用後的目標專案根目錄」，或統一調整。

### [MINOR] 7. skills/templates 是誤置於 skills 下的非 skill 目錄
- 位置：`.claude/skills/templates/`（README.md＋security-report-template.md），無 SKILL.md；README 仍寫 `.github/skills/templates/`。
- 建議：移出 skills/，更新 README 路徑。

### [MINOR] 8. skill-creator 與 skill-agent-design 高度重疊
- 兩者皆為「如何設計/建立 skill 與 agent」meta 指南，內容與 .github/ 路徑假設大量重疊。建議擇一保留或合併。

### [MINOR] 9. 五個 security skill 的 SKILL.md 與 README.md 內容重複
- 各 security skill 同時有 SKILL.md 與 README.md 且大量重疊；security-fast-scan 為串接其餘四者的協調層（刻意設計）。建議擇一為主。

### [MINOR] 10. .github/ 殘留與根層重複檔尚未清理
- `.github/copilot-instructions.md`、`.github/copilot-instructions0-old.md`、`.github/mcp-config.json` 殘留；根層另有內容**不同**的 copilot-instructions.md 與 CLAUDE_OLD.md（128KB）。
- 不確定性：是否刻意保留 Copilot 支援需使用者決定。

### [MINOR] 11. .claude/README.md 目錄結構過時
- 開頭目錄樹只列 commands/webapi.md、templates、command-processor.md，未涵蓋 agents/、skills/、commands/jb.md、settings.local.json。

### [MINOR] 12. jb.md 與 webapi.md 定位重疊
- 兩者皆屬 JobBank API 的 codegen／專案管理指令，功能定位相近。建議釐清分工或整併。

## settings.local.json（資訊性，無異常）
- 結構：permissions.allow（10 條 Bash 白名單）、enableAllProjectMcpServers（bool）、enabledMcpjsonServers（7 項：fetch、filesystem、playwright、memory、browserbase、context7、github）。
- 未見任何 token/password/金鑰；屬正常個人設定。
