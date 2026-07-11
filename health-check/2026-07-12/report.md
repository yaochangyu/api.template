# api.template 健檢總報告（2026-07-12 輪）

**健檢時間**：2026-07-12 01:05–02:00 GMT+8
**基準**：main @ `66e062c`（工作樹乾淨）
**方法**：4 個 sonnet sub-agent 實跑掃描（build/測試/依賴、程式碼、文件、安全）＋主對話（Fable）交叉判讀與根因確診。所有發現均附指令輸出或 `檔案:行號` 證據；推論處明標。
**範圍邊界**：診斷輪——未修改任何 main 上的既有檔案，僅新增本目錄報告。

---

## 結論（TL;DR）

**專案目前「可建置、不可信」**：build 0 error，但 main HEAD 的整合測試實跑 3/8，v2 Member API 三個端點實測全部 500。封存計畫 `66e062c`「100% - all tests passing」的宣告與可重現實況不符。共 P0×3、P1×4、P2×7、P3 若干。

## 嚴重度總表

| # | 發現 | 嚴重度 | 證據位置 | 詳見 |
|---|---|---|---|---|
| 1 | v2 Member API 全故障：生成的 `Contract.MemberController` 需要 `IMemberController`，DI 未註冊；手寫 Code First 版路由 `api/v2/Member` 與契約 `api/v2/members` 不符，測試命中生成版 | **P0** | logs/aspnet-202607120113.txt:8,21；Program.cs:65-66 | findings-code.md P0-1 |
| 2 | PipeWriter.UnflushedBytes 回歸：純 `Ok()` 端點在 TestServer 下 500。根因＝Mvc.Testing **8.0.10** 未實作 net10.0 STJ 要求（僅測試環境；Kestrel 無虞） | **P0** | IntegrationTest.csproj:17；logs/aspnet-202607120112.txt:46-48 | findings-code.md P0-2、findings-build-test.md §2.2 |
| 3 | Refit 7.2.1 **Critical** 漏洞（GHSA-3hxg-fxwm-8gf7） | **P0** | dotnet list --vulnerable | findings-build-test.md §3.1 |
| 4 | API First / Code First 混用（違反 CLAUDE.md「禁止混用」，且為 #1 的架構性根因） | P1 | Program.cs:64-66；ContractControllers.cs:143-176 | findings-code.md P1 |
| 5 | 套件漏洞 High×3（System.Security.Cryptography.Xml 8.0.2×2、Microsoft.OpenApi 2.0.0） | P1 | dotnet list --vulnerable | findings-build-test.md §3.1 |
| 6 | CLAUDE.md 自身導航連結失效（:28,:29）＋ development-rules.md 4 處 | P1 | test -e 逐一驗證 | findings-docs.md §2 |
| 7 | tree.md 與實況差 34 處（缺 26 含兩份核心必讀文件、過期 8），自報 167 實列 158 | P1 | git ls-files 比對 | findings-docs.md §3 |
| 8 | Production 例外訊息原樣外洩用戶端（不分環境） | P2 | ExceptionHandlingMiddleware.cs:79 | findings-security.md §3 |
| 9 | 單元測試直連 localhost:6379，Redis 缺席即 8/10 | P2 | 實跑輸出 | findings-build-test.md §2.1 |
| 10 | 套件層普遍停留 .NET 8 系列（TFM 已 net10.0）＝升級只完成一半 | P2 | dotnet list --outdated | findings-build-test.md §3.3 |
| 11 | ActionResult.cs 診斷 logging 殘留（dcbfa0f） | P2 | ActionResult.cs:18-38 | findings-code.md P2-1 |
| 12 | MemberChain.cs 死碼＋copy-paste bug（:54 誤比對 Email） | P2 | grep 交叉（疑似） | findings-code.md P2-2 |
| 13 | v1/v2 header 解析重複且行為不一致（v1 非法值擲例外、v2 安靜回退） | P2 | 兩檔逐行比對 | findings-code.md P2-3 |
| 14 | .archive/ 10/18 檔缺封存標記；1 檔名 completed 內文執行中 | P2 | 逐檔檢查 | findings-docs.md §4 |
| 15 | 上輪健檢報告 4 檔以現在式描述已消失的分支 | P2 | 4 檔行號 | findings-docs.md §5 |
| 16 | skills 內部連結壞 13+ 處（含 xxx.md 佔位符） | P2 | test -e | findings-docs.md §2 |
| 17 | nullable warning ~108 筆、xunit legacy、空測試 UnitTest1、README best-practices 壞連結、範本預設密碼等 | P3 | — | 各 findings |

安全掃描重點：**無真實憑證洩漏（P0=0）**；StackTrace 確認不外洩（Failure.Exception 有 [JsonIgnore]）。

## 關鍵翻案（與既有紀錄矛盾處，均有證據）

1. **「all tests passing」（66e062c）不成立**：本輪 fresh build 實跑 3/8。昨晚「8/8、4–5 秒」的紀錄，執行時間不足以含 Testcontainers 啟停（本輪 26.7s），推論跑到舊二進位（未能回溯證實）。
2. **「PipeWriter 是誤判、真因只有 DI」（obs 784）只對一半**：DI 是真 bug（#1），PipeWriter 也是真 bug（#2，測試層）。且兩者疊加——DI 例外後序列化 Failure 再觸發 PipeWriter，**外層堆疊遮蔽內層真因**，這正是升級時繞遠路的機制。
3. **「.NET 10 升級 100% 完成」實為 TFM 層完成**：套件層仍在 8.x 系列（#10），而 #2 正是套件層未升的直接後果。

## 如何驗證本報告（實測方式）

```bash
cd /mnt/d/lab/api.template/dotnet-project-template/src/be
# 1. build 基準
dotnet build JobBank1111.sln          # 預期 0 error / ~172 warning
# 2. 整合測試（需 Docker）
dotnet test JobBank1111.Job.IntegrationTest   # 預期 3/8，失敗含 IMemberController 與 PipeWriter 兩類（看 logs/aspnet-*.txt 原始例外）
# 3. 單元測試（本機無 Redis 時）
dotnet test JobBank1111.Job.Test              # 預期 8/10，敗因 UnableToConnect localhost:6379
# 4. 套件漏洞
dotnet list JobBank1111.sln package --vulnerable --include-transitive   # 預期 Refit Critical + 3 High
# 5. 文件連結（例）
test -e /mnt/d/lab/api.template/decision-framework.md; echo $?          # 預期 1（CLAUDE.md:28 壞連結）
```

## 交接包索引
- 各軌詳細發現：findings-build-test.md、findings-code.md、findings-docs.md、findings-security.md
- 關鍵決策與捨棄方案：decisions.md
- 修復執行計畫（Opus/Sonnet 照做粒度）：plan.md
- 風險與未解：risks-and-open-questions.md
- 工作筆記（learnings/死路）：notes.md
