# 健檢工作筆記（notes.md）

日期：2026-07-04
執行者：Claude（Fable 5 本體 + 4 個子 agent）

## 工作方式
- 本體：架構判讀、衝突裁決、報告撰寫
- 子 agent（並行）：
  - scan-docs（Explore）：根層指令文件衝突掃描
  - scan-claude-dir（Explore）：.claude/ 遷移後一致性
  - build-test（general-purpose/sonnet）：實跑 build + unit test（+ 嘗試 integration）
  - scan-template（Explore）：dotnet-project-template 結構掃描

## 本體親自驗證的事實（隨做隨記）
- [已驗證] 專案根目錄沒有進行中的 *.plan.md；`.archive/` 內 5 組 plan 均已完成歸檔。
- [已驗證] git 狀態：main 分支上有 **60 個檔案的 staged 未 commit 變更**（.github/agents+skills → .claude/ 遷移、刪除 12 個舊 agent、刪 .copilot/mcp-config.json、刪 .vscode/mcp.json），淨 -2823 行。另有 unstaged：.claude/settings.local.json、tree.md。
- [已驗證] CLAUDE.md 引用 `project-template/src/be/...`，但實際目錄是 `dotnet-project-template/`（待 scan-docs 統計失效引用數）。
- [已驗證] `.github/` 殘留：copilot-instructions.md、copilot-instructions0-old.md、mcp-config.json；根目錄另有一份 copilot-instructions.md → 三份 copilot 指令並存。
- [已驗證] dotnet-project-template 根層無 .sln（在 src/be 下，待 build-test 確認）。
- [已驗證] 根目錄同時有 CLAUDE.md (42K) 與 CLAUDE_OLD.md (128K)。
- [已驗證] doc/ 與 docs/ 在 dotnet-project-template 同時存在。

- [已驗證] **CLAUDE.md「開發指令」與 Taskfile.yml 嚴重不同步**：CLAUDE.md 列出 `task build`、`task test-unit`、`task test-integration`、`task ef-migration-add`、`task ef-database-update`，但 Taskfile.yml（dotnet-project-template/Taskfile.yml）完全沒有這些 task；實際只有 dev-init / redis-start / redis-admin-start / ef-codegen(-member) / codegen-api(-client/-server/-doc/-preview) / api-dev。CLAUDE.md 同時規定「EF Core 指令必須透過 Taskfile 執行」，但 Taskfile 根本沒有 migration task → 規則自相矛盾，照規則做會卡死。
- [已驗證] Taskfile `api-dev` 用 `dotnet watch run --local`，`--local` 不是 dotnet watch 的參數（會被傳給應用程式），需確認是否刻意。
- [已驗證] Taskfile 位於 dotnet-project-template/ 下，但 CLAUDE.md 的指令描述以 repo 根為敘事起點，未說明要先 cd。

## 死路 / 意外發現
- find 指令用 `-not -path './.git*'` 會誤排除 `.github`，改用 ls 驗證。（教訓：glob 排除要精確）
- 派工失誤：要求 Explore agent 寫檔，但 Explore 是唯讀（無 Write/Edit）。已改為請它們用訊息回傳內容、本體代為落檔。（教訓：派工前核對 agent 的工具權限；要產檔的任務用 general-purpose）

## 子 agent 回報結果（2026-07-04 收訖）
- scan-docs：6 項（3 major）→ findings-docs.md（本體代落檔）
- scan-claude-dir：12 項（1 blocker、5 major）→ findings-claude-dir.md（本體代落檔）
- scan-template：13 項（1 blocker、5 major）→ findings-template.md（agent 自行以 Bash 落檔成功）
- build-test 子任務**失敗**（session 額度用罄：resets 4:40pm Asia/Taipei）→ 改由本體實跑：build 0 error/129 warning、單元 10/10、整合 8/8（Docker 可用，5 秒完成，容器映像已快取）

## 額外教訓
- 子 agent 會共用 session 額度，四個並行派工把額度打爆；額度緊時應序列派工或縮減數量。
- Explore 雖無 Write 工具，但可用 Bash 寫檔（scan-template 就這樣繞過了）——派工時仍應明確指定回傳方式，不能依賴 agent 自己找路。
- blocker 驗證：ContractControllers.cs:67 需 ITagController、Program.cs 僅註冊 IMemberController（本體 grep 複核屬實）。

## 最終產出
- report.md（總報告）、decisions.md（D1–D9）、plan.md（P0–P9）、risks-and-open-questions.md、findings-*.md ×3
