# 修復 testcontainers 自動清理實作計畫

- [x] 釐清 `AfterTestRun`、Testcontainers 與 Reqnroll 的生命週期，找出殘留容器沒有自動清理的根因。（已確認專案使用 `Reqnroll.xUnit 2.1.1`，官方在 `2.4.1` 修正 xUnit async `[AfterTestRun]` 可能不會完整執行）
- [x] 實作可靠的清理策略，確保整合測試結束後不留下 SQL Server、Redis、MockServer 測試容器。
- [x] 重新執行整合測試並驗證測試通過後容器會自動清掉。
- [x] 更新 `tree.md` 與進度檔，記錄處理結果。
