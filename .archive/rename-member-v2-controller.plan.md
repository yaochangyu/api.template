⏹️ **Status**: COMPLETED (2026-07-10 23:29 GMT+8)

此計畫已全部執行完成，已封存至 .archive/

---

# Rename Member V2 Controller 實作計畫

- [x] 盤點 `MemberControllerImpl` 的使用點與相依關係，避免改名後漏掉 DI 或介面實作對應。
- [x] 將 `MemberControllerImpl` 重新命名為 `MemberV2ControllerImpl`，讓 v2 codegen route 的實作意圖更清楚。
- [x] 同步更新所有參考位置與專案結構文件 `tree.md`，避免名稱不一致。
- [x] 執行 build，確認改名後專案仍可正常編譯。
