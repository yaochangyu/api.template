⏹️ **Status**: COMPLETED (2026-06-07 18:58 GMT+8)

此計畫已全部執行完成，已封存至 .archive/（標記補於 2026-07-12 健檢輪 S7）
詳見 Git commit: 8a4ab45

---

# fix-test-environment 進度追蹤

- [x] 盤點殘留容器與 Redis 狀態。
- [x] 清理整合測試衝突容器。
- [x] 啟動本機 Redis。
- [x] 重跑單元測試與整合測試。

## 結果

- 單元測試：10/10 通過
- 整合測試：已排除 `sql2019`/`mockserver` 固定名稱衝突與 Redis 連線失敗；目前剩 3 個功能性失敗

## 失敗紀錄

- 方法：直接在未清理殘留容器的情況下重跑整合測試
  - 結果：失敗
  - 原因：固定容器名稱 `sql2019`、`mockserver` 被前一次測試殘留占用
  - 下次避免：重跑前先確認殘留容器是否存在，或修正測試 teardown
- 方法：在未啟動本機 Redis 的情況下重跑單元測試
  - 結果：失敗
  - 原因：Redis 單元測試使用 `localhost:6379`
  - 下次避免：先啟動 `docker compose up -d redis`
- 方法：清理環境後重跑整合測試
  - 結果：仍有 3 個失敗
  - 原因：已非環境問題，改為 API 回應內容/格式不符
- 方法：加入 container `DisposeAsync`
  - 結果：未有效清掉 SQL Server / Redis 測試容器
  - 原因：目前專案的 Testcontainers 清理不適合靠這條路徑解決
  - 下次避免：不要再用 `DisposeAsync` 當主要修復手段
- 方法：改為 Testcontainers 自動命名，移除固定 `sql2019` / `mockserver`
  - 結果：成功排除下一輪測試的容器撞名問題
  - 原因：即使容器稍後才被清理，也不會先被固定名稱卡住
- 方法：加入 `StopAsync + AutoRemove`
  - 結果：仍未讓 SQL Server / Redis 在測試結束後自動消失
  - 原因：`AfterTestRun` / Testcontainers cleanup 行為仍需進一步追
  - 下次避免：不要假設只加這兩項就能解掉殘留容器
