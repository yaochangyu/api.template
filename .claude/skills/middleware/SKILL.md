---
name: middleware
description: 中介軟體實作技能，協助開發者實作符合專案規範的中介軟體，包含 TraceContext 管理、Exception Handling、Request Logging 與管線配置。
---

# Middleware Skill

## 描述
中介軟體實作技能，協助開發者實作符合專案規範的中介軟體，包含 TraceContext 管理、Exception Handling、Request Logging 等。

## 職責
- TraceContext 管理與注入
- Exception Handling 實作
- Request/Response Logging
- 中介軟體管線配置

## 中介軟體管線順序

```csharp
app.UseMiddleware<MeasurementMiddleware>();       // 計時
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 例外處理
app.UseMiddleware<TraceContextMiddleware>();      // 追蹤內容
app.UseMiddleware<RequestParameterLoggerMiddleware>(); // 請求日誌
```

## 核心中介軟體

### 1. TraceContextMiddleware
- 設定 TraceId（從請求標頭或自動產生）
- 處理使用者身分驗證
- 注入 TraceContext 到 DI 容器

### 2. ExceptionHandlingMiddleware
- 捕捉未處理的系統例外
- 記錄錯誤日誌
- 統一回應格式

### 3. RequestParameterLoggerMiddleware
- 記錄請求參數（路由、查詢、標頭、本文）
- 僅在成功完成時記錄
- 排除敏感資訊

## 參考文件
- [中介軟體架構](./references/middleware-architecture.md)

## 範本檔案
- [中介軟體範本](./assets/middleware-template.cs)
