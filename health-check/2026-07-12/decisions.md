# 關鍵決策（2026-07-12 健檢輪）

每條含：決策、為什麼、捨棄的替代方案。修復執行順序見 plan.md。

## D1：v2 收斂為純 Code First（openapi.yml 移除 v2 paths）
**決策**：`doc/openapi.yml` 刪除 `/api/v2/members*` 三個 path → NSwag 重新生成（生成物只剩 v1）→ 手寫 `MemberController` 路由對齊原契約路徑（`api/v2/members`、`:cursor`/`:offset` custom method 形式）→ BDD 測試不改，以 8/8 為驗收。
**為什麼**：
- `25fca42` 已表態 v2 走 Code First，是既定方向；本決策只是把「生成管線仍在產 v2 API First Controller」這個尾巴收掉，根治 P0-1。
- 範本教學價值最大化：v1 完整示範 API First（契約→生成→實作介面）、v2 完整示範 Code First，兩者各自成立。
- 路由對齊契約（複數小寫＋冒號 custom method）：測試與 openapi 語意是對的，錯的是手寫路由 `[Route("api/v2/[controller]")]`。
**捨棄**：
- ~~註冊 IMemberController 走滿 API First~~：Code First 示範消失，且 `7b21f2d` 已試過又被翻掉，反覆。
- ~~兩套並存＋補 DI~~：同一資源兩套路由，混亂且持續違反「禁止混用」。

## D2：CLAUDE.md 增補「範本例外」註記，而非刪掉 v1 或 v2
**決策**：在 CLAUDE.md 與 development-rules.md 的「禁止混用」條款加註：本範本刻意以 v1 示範 API First、v2 示範 Code First 供教學對照；使用者的實際專案仍須全專案擇一。
**為什麼**：規則與範本實況目前正面矛盾（README:114 說不得混用，README:118-119 又並列兩種實例），不註記則每次健檢／每個接手 AI 都會再撞一次。
**捨棄**：~~刪除其中一種模式~~：範本存在目的就是展示兩種流程（CLAUDE.md 的 API 開發流程決策章節依賴這兩個實例）。

## D3：套件升級分兩批——低風險 Microsoft 系先行、Refit 大版本獨立 spike
**決策**：
- 批 A（低風險）：Mvc.Testing、TimeProvider.Testing、EFCore 系、Caching 系、Data.SqlClient、System.Security.Cryptography.Xml、Microsoft.OpenApi、Azure.Identity → 各升至 10.x／最新穩定。
- 批 B（高風險、獨立 spike）：Refit(.HttpClientFactory) 7.2.1 → 13.1.0（解 Critical GHSA-3hxg-fxwm-8gf7）。大版本跳 6 版，且 Contract 生成物（Refitter）相依其 API，先 spike 驗證生成管線相容再併。
**為什麼**：批 A 同源同節奏（8.x→10.x），風險低、一次解掉 P0-2（Mvc.Testing）與多數 NU190x；批 B 混進去會讓失敗難歸因。
**捨棄**：~~全部一次升~~（失敗難歸因）；~~只升有漏洞的~~（8.x 套件跑在 net10.0 上正是 P0-2 成因，半套升級是這次事故的直接教訓）。

## D4：P0-2 的修法是升 Mvc.Testing，不是恢復任何 JSON workaround
**決策**：不重新引入 SynchronousJsonOutputFormatter／Newtonsoft 類 workaround；唯一修法＝Mvc.Testing 8.0.10 → 10.0.x（TestServer 實作 UnflushedBytes）。
**為什麼**：根因已確診在測試工具層（IntegrationTest.csproj:17；Kestrel runtime 不受影響）；workaround 攻防戰（f626a90→cf31f33）已證明繞根因的修補會反覆。
**捨棄**：~~自訂 formatter 繞過~~（動 production 程式去遷就測試工具，方向顛倒）。

## D5：Production 例外訊息改環境閘門
**決策**：ExceptionHandlingMiddleware.CreateFailure 在非 Development 環境回泛用訊息（保留 TraceId 供查 log），Development 維持原樣；log 端不變（已完整）。
**為什麼**：`exception.Message` 原樣出站可能含連線字串片段／SQL 錯誤（P2）；TraceId 已足夠關聯 server log。
**捨棄**：~~整包 ProblemDetails 重構~~（超出健檢範圍，屬未要求的功能）。

## D6：文件修復「批量小修」，舊報告「加橫幅不重寫」
**決策**：19+ 壞連結逐一修正；tree.md 重產；.archive/ 10 檔補標記；上輪 health-check/ 4 檔僅在檔頂加「⚠️ 2026-07-12：本報告描述之分支已併入 main，內容為歷史紀錄」橫幅。
**為什麼**：舊報告是歷史紀錄，重寫會破壞其作為當時證據的價值；橫幅解決誤導問題。
**捨棄**：~~搬 health-check/ 舊檔進 .archive/~~（路徑被 memory 與多處引用，搬移會斷鏈；只加橫幅零風險）。

## D7：單元測試 Redis 案例遷往整合測試專案（Testcontainers）
**決策**：CacheProviderFactoryTest 兩個 Redis 案例移至 IntegrationTest（或改用 Testcontainers 起 Redis），單元測試專案恢復無環境依賴。
**為什麼**：專案測試策略明定真依賴走 Testcontainers；現況 CI 無常駐 Redis 即紅，「單元測試」名不符實。
**捨棄**：~~mock Redis~~（違反本專案「優先真實容器、避免 Mock」的測試策略）。

## D8：新增啟動期 Controller DI 契約測試
**決策**：加一個整合測試：反射列舉所有 ControllerBase 衍生類，逐一驗證建構子參數皆可自 DI 解析。
**為什麼**：P0-1 的缺口類型（ValidateOnBuild 管不到 Controller）已兩度造成回歸（7b21f2d 修過、25fca42 又翻掉），值得永久性防線；成本一個測試檔。
**捨棄**：~~只靠文件提醒~~（已證明擋不住）。

## D9：本輪產出放新分支 `health-check-20260712`，不動 main、不 push
**為什麼**：使用者邊界「不動 main」；上輪慣例 push/MR 需明確授權。
