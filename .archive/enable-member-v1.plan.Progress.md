⏹️ **Status**: COMPLETED (2026-06-07 18:58 GMT+8)

此計畫已全部執行完成，已封存至 .archive/（標記補於 2026-07-12 健檢輪 S7）
詳見 Git commit: 8a4ab45

---

# enable-member-v1 進度追蹤

- [x] 已確認採用「沿用目前 Handler/Failure 處理方式，只新增 v1 路由」。
- [x] 啟用 `MemberV1Controller` 並接到現有 `MemberHandler`。
- [x] 更新 `tree.md` 與計畫檔狀態。
- [x] 執行 build。

## 測試結果

- [x] 已執行單元測試：`dotnet test JobBank1111.Job.Test/JobBank1111.Job.Test.csproj --no-build`
- [x] 已執行整合測試：`dotnet test JobBank1111.Job.IntegrationTest/JobBank1111.Job.IntegrationTest.csproj --no-build`

## 失敗紀錄

- 方法：直接執行單元測試專案
  - 結果：失敗
  - 原因：`CacheProviderFactoryTest` 的 Redis 測試連不到 `localhost:6379`
  - 下次避免：在未先啟動本機 Redis 前，不要重跑這組 Redis 相關單元測試
- 方法：直接執行整合測試專案
  - 結果：失敗
  - 原因：Testcontainers 建立 SQL Server 容器時，固定名稱 `/sql2019` 已被既有容器占用
  - 下次避免：在未先清掉或更名既有 `sql2019` 容器前，不要重跑整合測試
