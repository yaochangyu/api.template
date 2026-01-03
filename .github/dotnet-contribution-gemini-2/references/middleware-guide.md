# 中介軟體架構與實作指引

本文件詳述專案的中介軟體管線架構、職責劃分與最佳實務。

## 中介軟體管線架構與職責

#### 管線順序與責任劃分
1. **MeasurementMiddleware**: 最外層，度量與計時，包覆整體請求耗時
2. **ExceptionHandlingMiddleware**: 捕捉未處理的系統層級例外，統一回應格式
3. **TraceContextMiddleware**: 設定追蹤內容與身分資訊（如 TraceId、UserId）
4. **RequestParameterLoggerMiddleware**: 在管線尾端於成功完成時記錄請求參數

🧩 程式碼為準（Program.cs）
```csharp
// 管線順序：Measurement → ExceptionHandling → TraceContext → RequestParameterLogger
app.UseMiddleware<MeasurementMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TraceContextMiddleware>();
app.UseMiddleware<RequestParameterLoggerMiddleware>();
```

#### 職責分離原則
- **例外處理**: 僅在 `ExceptionHandlingMiddleware` 捕捉系統例外
- **追蹤管理**: 所有 TraceContext 相關處理集中在 `TraceContextMiddleware`
- **日誌記錄**: 分別在例外情況和正常完成時記錄，避免重複
- **請求資訊**: 使用 `RequestInfoExtractor` 統一擷取請求參數

## RequestInfoExtractor 功能
1. **路由參數**: 擷取 URL 路由中的參數
2. **查詢參數**: 擷取 URL 查詢字串參數
3. **請求標頭**: 擷取 HTTP 標頭，自動排除敏感標頭
4. **請求本文**: 對於 POST/PUT/PATCH 請求，擷取請求本文內容並嘗試解析 JSON
5. **基本資訊**: 記錄 HTTP 方法、路徑、內容類型、內容長度等

## 中介軟體最佳實務原則
- **專一職責**: 每個中介軟體專注於單一關注點
- **避免重複**: 透過管線設計避免重複處理和記錄
- **統一格式**: 所有請求資訊記錄使用相同的資料結構
- **效能考量**: 只有在需要時才擷取請求本文
- **錯誤容錯**: 記錄過程中發生錯誤不影響業務邏輯執行
