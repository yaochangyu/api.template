# 核心開發原則

本文件闡述專案的核心開發原則，包括不可變物件設計與架構守則。

## 不可變物件設計 (Immutable Objects)
- 使用 C# record 類型定義不可變物件，例如 `TraceContext`
- 所有屬性使用 `init` 關鍵字
- 避免在應用程式各層間傳遞可變狀態

## 架構守則
- 業務邏輯層不應直接處理 HTTP 相關邏輯
- 所有跨領域關注點（如身分驗證、日誌、追蹤）應在中介軟體層處理
- 使用不可變物件傳遞狀態
- 透過 DI 容器注入 TraceContext

## 用戶資訊管理
- **不可變性原則**: 確保物件的不可變，身分驗證後的用戶資訊存放在 TraceContext
- **集中處理**: 集中在 TraceContextMiddleware 處理
- **依賴注入**: 透過 IContextSetter 設定用戶資訊，透過 IContextGetter 取得
