# findings-docs.md — AI 指令文件衝突與過時掃描

- 掃描者：scan-docs（Explore agent，唯讀）；本檔由主對話代為落檔
- 範圍：CLAUDE.md / CLAUDE_OLD.md / copilot-instructions 三份 / tree.md / .github/mcp-config.json
- 發現總數：6（blocker 0、major 3、minor 3）

## F1 — CLAUDE.md 範例檔路徑前綴全失效（major）
- 檔案/行：CLAUDE.md:144, 284-286, 317, 328, 338, 357, 370, 567, 595, 606, 607, 639, 697, 717, 781, 782, 819-821, 836, 875-878, 906, 941, 1004, 1027, 1030, 1033, 1036-1038, 1041-1043, 1046, 1049, 1052, 1053
- 問題：CLAUDE.md 用 `project-template/src/be/...` 作範例檔連結前綴，且 `doc/openapi.yml`、`docker-compose.yml` 寫成根層相對路徑，但實際全在 `dotnet-project-template/` 下。
- 量化：markdown 連結型失效 42 筆（不重複目標 19 個）；其中 project-template/ 37、doc/openapi.yml 4、docker-compose.yml 1。
- 驗證：`test -e project-template/...` 全 MISSING；補 `dotnet-project-template/` 前綴後全 EXISTS。
- 影響：整份「附錄：快速參考」與各章「📝 實作參考」連結全部撲空。

## F2 — 三份 copilot-instructions 並存且分歧（major）
- 檔案：`copilot-instructions.md`（根，20 行）、`.github/copilot-instructions.md`（30 行）、`.github/copilot-instructions0-old.md`（19 行）
- 關係（diff/git log 驗證）：
  - 根版與 .github/copilot-instructions0-old.md 幾乎逐字相同，差別僅根版多最後一行（:20「列出每一次思考、動作，用了哪些 mcp、agent、skill??」，git 84f3758 補上）。兩者為舊「# 注意事項」格式。
  - `.github/copilot-instructions.md` 是重寫版（# 基本規範 / # 實作流程規範 / # 技術規範）。
- 衝突：
  1. 位置錯誤：GitHub Copilot 只自動讀 `.github/copilot-instructions.md`；根層那份不會被讀，卻是唯一含「記錄 mcp/agent/skill」規則的版本 → 該規則實際不生效。
  2. 內容分歧、無單一真相：現行位置的重寫版沒有「記錄 mcp/agent/skill」這條，根版有；無法判定何者權威。
  3. `.github/copilot-instructions0-old.md` 檔名自稱 old，為應刪殘留。

## F3 — CLAUDE_OLD.md 孤兒舊版（major）
- 檔案：CLAUDE_OLD.md（3702 行）vs CLAUDE.md（1092 行）
- 引用：全 repo 無檔案引用 CLAUDE_OLD.md（僅 tree.md 列檔）；CLAUDE_OLD.md 無版本/日期標記（CLAUDE.md 有「文件版本 2.1、最後更新 2025-12-16」）。
- 有無矛盾：抽查未見直接矛盾——中介軟體管線順序兩份一致且符合 Program.cs:90-93；FailureCode 列舉兩份一致且符合 FailureCode.cs（9 值）。
- CLAUDE_OLD.md 獨有內容（CLAUDE.md 已刪）：EF migration 指令範例（CLAUDE_OLD.md:968-983）、Dockerfile/compose/CI-CD/k8s YAML（3146-3517）、整節「## 日誌與安全指引」（2534）、獨立「## Repository Pattern 設計哲學」節（3518）與大量 code sample。
- 附帶：CLAUDE_OLD.md 路徑用 `src/be/...`（無 dotnet-project-template/ 前綴，20 處，如 :595-597），同樣失效。
- 影響：兩份 CLAUDE 並存，易誤讀未維護舊版；刪除前需決定哪些獨有內容回填。

## F4 — tree.md 過時（minor）
- 問題：tree.md:5 宣稱「總計 166 個檔案」，實際（排除 bin/obj/.git）184 個。未收錄 `.github/copilot-instructions0-old.md`、`health-check/`、`.omc/`。已反映 .claude/ 遷移後結構，非全面過時。
- 交叉：兩份 copilot-instructions 都要求「異動檔案時同步更新 @tree.md」，目前未落實。

## F5 — .github/mcp-config.json 位置無載入 + skill 文件過時（minor）
- 內容：單一 MCP server `context7-http`（HTTP，url https://mcp.context7.com/mcp，tools *）。
- 問題：
  1. 位置無工具自動載入：GitHub Copilot 讀 `.copilot/mcp-config.json` 或 `.vscode/mcp.json`（兩者已於本次 staged 變更刪除）；Claude 讀根層 `.mcp.json`（不存在，只有 dotnet-project-template/.mcp.json）→ 實質孤兒。
  2. skill 文件過時：.claude/skills/skill-agent-design/skill.md:26,181 與 .claude/skills/skill-creator/SKILL.md:421,546 仍把 MCP 位置寫成已刪的 `.copilot/mcp-config.json`。

## F6 — CLAUDE.md env/.template-config.json 位置語意不符（minor）
- 檔案/行：CLAUDE.md:67, 75, 86, 106, 108（及 :1013 的 env/local.env）
- 問題：CLAUDE.md 以根層 `env/.template-config.json` 做專案狀態檢測，但根層無 env/；實際 env 在 `dotnet-project-template/env/local.env`。設定檔屬初始化時才建立（允許現不存在），但敘述位置與真實 env 位置不符，狀態檢測會落在錯目錄。

## 交叉引用
- CLAUDE.md「開發指令」列的 task build/test-unit/test-integration/ef-migration-add/ef-database-update 在 dotnet-project-template/Taskfile.yml 不存在，與「EF 指令必須走 Taskfile」規則自相矛盾（主對話已獨立驗證）。

## 查不了 / 需作者確認
- 哪份 copilot-instructions 為現行權威（F2）。
- CLAUDE_OLD.md 是待刪或待回填後刪、保留哪些獨有章節（F3）。
- .github/mcp-config.json 是否為某自訂流程刻意讀取（F5）。
