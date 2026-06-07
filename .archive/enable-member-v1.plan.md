# 啟用 Member v1 API 實作計畫

- [x] 釐清 v1 要保留的行為與輸出格式，避免啟用後與既有 v2 或舊版範例語意不一致。（已確認：沿用目前 Handler/Failure 處理方式，只新增 v1 路由）
- [x] 調整 `MemberController.cs`，讓 `api/v1/members`、`api/v1/members:cursor`、`api/v1/members:offset` 正式註冊為可用端點，並正確呼叫目前的 `MemberHandler` 方法。
- [x] 補齊必要的型別轉換與回應處理，確保 v1 能重用現有業務邏輯而不破壞編譯。
- [x] 更新 `tree.md` 反映檔案異動，避免專案結構文件失真。
- [x] 執行 build 驗證啟用後的專案仍可正常編譯。
