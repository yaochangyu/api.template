# 健檢工作筆記（2026-07-12 輪）

**狀態**：診斷完成。產出：report.md（總表）、decisions.md（D1–D9）、plan.md（S1–S9）、risks-and-open-questions.md、findings-*.md ×4。

## 背景
- 上輪健檢（2026-07-04, P0–P9）已於 7/8 併入 main（obs 133），不重做。
- 之後 main 上發生 .NET 10 升級（15bcdf5..66e062c），過程反覆：
  - JSON workaround 加了又撤（f626a90 Newtonsoft → c7787ed/eee45df SynchronousJsonOutputFormatter → cf31f33 全撤）
  - OpenAPI 移除（b398ea9）又 revert（022d506）
  - dcbfa0f 在 ActionResult.cs 加了「詳細診斷 logging」——需確認是否殘留
  - 真因是 DI 註冊缺失（obs 780/784），非序列化
- 本輪重點：.NET 10 升級後殘留、重複、衝突點。診斷輪，不改碼。

## 派工紀錄
- A build-test-deps（sonnet）：build/測試實跑 + 套件漏洞掃描 → findings-build-test.md
- B code-scan（sonnet）：升級殘留、v1/v2 重複、DI 一致性 → findings-code.md
- C docs-scan（sonnet）：README 雙語同步、CLAUDE.md 連結、tree.md 新鮮度、skills 索引 → findings-docs.md
- D security-scan（sonnet）：secrets、config、.gitignore 覆蓋 → findings-security.md

## learnings / 死路 / 意外發現
- 【P0 根因確診，主對話 01:20】整合測試 5/8 失敗（PipeWriter.UnflushedBytes）的根因：
  `JobBank1111.Job.IntegrationTest.csproj:17` Mvc.Testing **8.0.10** ＋ net10.0 STJ 要求
  PipeWriter.UnflushedBytes，8.x TestServer 的 ResponseBodyPipeWriter 未實作 → 僅 TestServer 炸，
  Kestrel runtime 不受影響。修法：Mvc.Testing 升 10.0.x（一行）。**已驗證憑據：csproj 版本＋堆疊吻合；
  修復效果未驗證（診斷輪不改碼）。**
- 【矛盾釐清】昨晚 obs 770/773/778「8/8 通過、4–5 秒」vs 今晚實跑 26.7 秒 3/8：
  4 秒不可能含 Testcontainers 啟停；obs 770 又自述當時 --no-build 參數反覆出錯。
  推論（未能回溯證實）：昨晚跑到含 workaround 的舊二進位。教訓：**驗證宣告必須附 fresh build＋
  執行時間合理性檢查**。obs 784「PipeWriter 是誤判、真因只有 DI」的結論有一半錯了——DI 是真 bug，
  但 PipeWriter 也是真 bug（測試層）。memory「PipeWriter 問題消除」需修正。
- 【P0】Refit 7.2.1 Critical 漏洞（GHSA-3hxg-fxwm-8gf7）；套件層普遍停在 .NET 8 系列＝
  「升級 100% 完成」實為只完成 TFM 層。
- 【判讀修正】code-scan 讀 server log 發現 member 端點原始例外是 `IMemberController` DI 缺失，
  PipeWriter 是它序列化 Failure 時的第二層例外 → **PipeWriter 遮蔽 DI 真因**。我稍早「5 敗全是
  PipeWriter」的歸因不完整；兩個 P0 並存。手寫 v2 Controller 路由 `api/v2/Member` 與契約
  `api/v2/members` 不符，測試打的是 NSwag 生成版——25fca42「恢復 Code First」只改了一半。
- 【死路紀錄】曾想派 Explore 做程式碼掃描——Explore 不能寫檔也罷，實際連 general-purpose
  subagent 寫報告 md 都被 harness 擋，一律改「agent 回文字、主對話落地」。
- 【流程教訓】完成宣告必須附 fresh build 證據＋執行時間合理性（4 秒帶 Testcontainers 不可能）。
- 【harness 限制】subagent 寫 health-check/2026-07-12/findings-*.md 被政策擋下 → 改為 agent 文字回報、主對話代筆落地。後續派工模板應預期此限制。
- 【候選發現】歷史測試輸出顯示 JobBank1111.Job.Test（單元測試專案）的 CacheProviderFactoryTest 直連 localhost:6379（RedisCacheProvider.cs:46 經由真連線），Redis 沒開就 2/10 失敗 → 「單元測試」不隔離，與專案測試策略（真依賴應走 Testcontainers）衝突。待 build-test-deps 實跑交叉確認。
- security-scan 完成：P0=0、P3=4（env/local.env 範本密碼、.mcp.json 佔位符、redis 無密碼、AllowedHosts=*）。未展開項：ExceptionHandlingMiddleware 是否洩漏 StackTrace → 入 risks。
- 本體獨立判讀（2026-07-12 01:10，未與 agent 交叉比對前）：
  - Program.cs:64-66 v1 API First + v2 Code First 並存 ↔ CLAUDE.md「禁止混用」正面衝突。
    上輪 P7 是刻意決策（v1 入契約），但 CLAUDE.md 規則沒有同步加例外註記 → 規則與實況漂移。
  - Program.cs:44 Seq URL 硬編 localhost:5341（Testing 環境有排除，但 URL 本身該進 config）。
  - MemberController.cs:28-46 與 68-92：header 解析邏輯重複兩份（inline vs helper）。
  - MemberController 路由用 `[HttpGet(":cursor")]` 字面冒號段（Google AIP custom method 風格？）——需確認是刻意還是誤寫。
- 上輪健檢報告（health-check/ 根層 9 檔）描述的分支狀態已過時（health-check-fixes 已併入 main），會誤導接手者。
