# 文件一致性掃描結果（2026-07-12，docs-scan agent／sonnet 實跑，主對話代筆落地）

**摘要：壞連結 19+ 處（跨 10 檔，含 CLAUDE.md 自身 2 處 P1）；tree.md 與實況差 34 處（P1）；.archive/ 10/18 檔缺封存標記（P2）；上輪健檢報告 4 檔敘述已過時（P2）；README 雙語同步良好（P3）**

## 1. README 雙語同步（P3 同步良好／P1 壞連結）
- 兩檔各 295 行、21 標題逐行對應，1:1 忠實翻譯，無單邊獨有章節。
- 但共用失效連結：`./dotnet-project-template/best-practices.md` 已不存在（兩檔各 2 處＋目錄結構圖 1 處）。該檔實際已改名或移除（tree.md 過期條目顯示曾有 `最佳實踐.md`）。

## 2. CLAUDE.md 與 .claude/ 連結有效性（P1×6、P2 多處）
P1（核心導航文件自身壞連結）：
- `CLAUDE.md:28` `./decision-framework.md`、`CLAUDE.md:29` `./skills/api-development/SKILL.md` — 均漏 `.claude/` 前綴。
- `.claude/development-rules.md:87,92,104,126` — 4 處 `../DECISION-FRAMEWORK.md#...`：多跳一層＋大小寫不符（實際為 `.claude/decision-framework.md`）。

P2（skills 內部）：
- api-development、repository-design 的 references 指向 src 路徑漏 `dotnet-project-template/` 段。
- project-init references 的 `../../../CLAUDE.md` 少一層。
- bdd-testing、ef-core 的 SKILL.md 指向不存在的 references 檔名。
- caching-strategy（3 處）、skill-agent-design（含 `xxx.md` 佔位符殘留）、skill-creator（2 處）— references/ 目錄根本不存在。
- SKILL 索引表 10 目錄本身存在；另有 7 個 skills 目錄未收錄於索引（留意，非錯誤）。

## 3. tree.md 新鮮度（P1）
- git ls-files＝176 檔；tree.md 自稱 167、實列 158（自報數與內容自相矛盾）。
- 缺漏 26：含 `.claude/decision-framework.md`、`.claude/development-rules.md`（CLAUDE.md 路由的兩份必讀文件！）、.archive/ 9 檔、根層 README×2＋.gitignore、MemberController.cs、8 個 skills references。
- 過期 8：`.claude/README.md`、`best-practices.md`、`MemberV2ControllerImpl.cs`（已改名 MemberV1ControllerImpl.cs）、`如何產生agent.md`、`最佳實踐.md` 等。

## 4. plan 封存狀態（P2）
- 根目錄無殘留 *.plan.md ✅。
- .archive/ 18 檔中 10 檔缺 `⏹️ Status: COMPLETED` 檔頭標記（enable-member-v1、fix-integration-failures、fix-testcontainers-cleanup、fix-test-environment 各 .plan+.Progress、P2.1-bdd-testing、rename Progress）。
- `P2.1-bdd-testing-PLAN.completed.md` 檔名標 completed、內文卻仍是「進度: [執行中]」＋未勾選項 → 需人工複核。

## 5. 上輪健檢報告時效性（P2）
`health-check/` 根層 4 檔（decisions.md:50、execution-summary.md:3、notes.md:16、plan.md:4）以現在式描述 `health-check-fixes` 分支，該分支已於 `4d5572a` 併入 main 並刪除——接手者可能誤判為進行式。

## 6. doc/ vs docs/（P3）
單數 doc/＝API First 規格輸入（openapi.yml）；複數 docs/＝BDD 測試指南。內容零重疊，僅命名易混淆；README 目錄圖漏列 docs/。

## 7. OpenAPI/Swagger 敘述一致性（P3，通過）
Program.cs:62/81 AddOpenApi/MapOpenApi；全庫無 Swashbuckle 殘留；README 敘述與實況一致。

## 附註（主對話交叉觀察）
README:114「必須擇一，不得混用（API First / Code First）」，但同一張表（README:118-119）以 `MemberV1ControllerImpl.cs`（API First）與 `MemberController.cs`（Code First）並列為本專案實例——**範本自身同時示範兩種模式，與「不得混用」規則表面矛盾**，需一個明文的「範本例外」決策（見 decisions.md）。
