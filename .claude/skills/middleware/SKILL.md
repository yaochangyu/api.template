---
name: middleware
description: 中介軟體實作技能，協助開發者實作符合專案規範的中介軟體，包含 TraceContext 管理、Exception Handling、Request Logging 與管線配置。
---

### ⚠️ 前置條件
本 SKILL 須搭配閱讀：[開發規則](../../development-rules.md)

# Middleware Skill

## 描述
中介軟體實作技能，協助開發者實作符合專案規範的中介軟體，包含 TraceContext 管理、Exception Handling、Request Logging 等。

## 職責
- TraceContext 管理與注入
- Exception Handling 實作
- Request/Response Logging
- 中介軟體管線配置

## 注意事項

### Middleware 與 API 開發流程無關

中介軟體是應用層級的基礎設施，不受「API First vs Code First」選擇影響：
- **API First**：自動產生的 Controller 在已配置的中介軟體管線中執行
- **Code First**：手動實作的 Controller 在已配置的中介軟體管線中執行
- **結果**：中介軟體配置代碼完全相同

因此本 SKILL 未區分 API 開發流程，所有中介軟體實作指導均適用於兩種方式。

👉 **API 開發流程選擇** → 參考 [`/api-development` SKILL](../api-development/SKILL.md)

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

## 實作參考

取得中介軟體實作範例（透過 FileResolver）：
```bash
node .claude/skills/shared/FileResolver.js get-content \
  JobBank1111.Job.WebAPI/TraceContextMiddleware.cs
```
