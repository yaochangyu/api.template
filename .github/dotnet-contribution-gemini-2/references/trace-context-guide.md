# 追蹤內容管理 (TraceContext) 指引

本文件說明專案中 `TraceContext` 的集中式管理架構、生命週期與服務注入方式。

## 集中式管理架構
- **統一處理點**: 所有追蹤內容與使用者資訊統一在 `TraceContextMiddleware` 中處理
- **不可變性**: `TraceContext` 使用 record 定義，包含 `TraceId` 與 `UserId` 等不可變屬性
- **身分驗證整合**: 在 `TraceContextMiddleware` 中統一處理使用者身分驗證

## 生命週期與服務注入
- **生命週期**: 透過 `AsyncLocal<T>` 機制確保 TraceContext 在整個請求生命週期內可用
- **服務注入**: 使用 `IContextGetter<T>` 與 `IContextSetter<T>` 介面進行依賴注入
- **TraceId 處理**: 從請求標頭擷取或自動產生 TraceId
- **回應標頭**: 自動將 TraceId 加入回應標頭供追蹤使用

## 核心開發原則
- **不可變物件設計 (Immutable Objects)**: 使用 C# record 類型定義不可變物件，例如 `TraceContext`。所有屬性使用 `init` 關鍵字，避免在應用程式各層間傳遞可變狀態。
- **用戶資訊管理**: 確保物件的不可變性，身分驗證後的用戶資訊存放在 TraceContext。
- **集中處理**: 集中在 `TraceContextMiddleware` 處理。
- **依賴注入**: 透過 `IContextSetter` 設定用戶資訊，透過 `IContextGetter` 取得。
