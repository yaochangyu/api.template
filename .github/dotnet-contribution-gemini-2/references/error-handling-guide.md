# 錯誤處理與回應管理指引

本文件描述了專案如何使用 Result Pattern 進行錯誤處理，以及各分層的錯誤處理策略。

## Result Pattern 設計

#### 核心架構
- **Result 套件**: 使用 `CSharpFunctionalExtensions` 3.1.0 套件
- **應用範圍**: Repository 層和 Handler 層必須使用 `Result<TSuccess, TFailure>` 作為回傳類型
- **映射規則**: 使用 `FailureCodeMapper` 將錯誤代碼映射至 HTTP 狀態碼

#### FailureCode 列舉
```csharp
public enum FailureCode
{
    Unauthorized,        // 未授權存取
    DbError,            // 資料庫錯誤
    DuplicateEmail,     // 重複郵件地址
    DbConcurrency,      // 資料庫併發衝突
    ValidationError,    // 驗證錯誤
    InvalidOperation,   // 無效操作
    Timeout,           // 逾時
    InternalServerError, // 內部伺服器錯誤
    Unknown            // 未知錯誤
}
```

#### Failure 物件結構
- **Code**: 錯誤代碼
- **Message**: 例外的原始訊息
- **TraceId**: 追蹤識別碼
- **Exception**: 原始例外物件（不序列化到客戶端）
- **Data**: 結構化資料

## 分層錯誤處理策略

#### 業務邏輯錯誤處理 (Handler 層)
- 使用 Result Pattern 處理預期的業務邏輯錯誤
- 回傳適當的 HTTP 狀態碼 (400, 401, 404, 409 等)
- 不應讓業務邏輯錯誤流到系統例外處理層

#### 系統層級例外處理 (ExceptionHandlingMiddleware)
- 僅捕捉未處理的系統層級例外
- 使用結構化日誌記錄例外詳細資訊
- 將系統例外轉換為標準化的 `Failure` 物件回應
- 統一設定為 500 Internal Server Error

## 錯誤處理最佳實務原則
- **不要重複拋出例外**: 處理過的例外不應再次 throw
- **統一錯誤碼**: 使用 `nameof(FailureCode.*)` 定義錯誤碼
- **例外封裝規則**: 所有捕捉到的例外都必須寫入 `Failure.Exception` 屬性
- **包含追蹤資訊**: 確保所有 Failure 物件都包含 TraceId
- **安全回應**: 不洩露內部實作細節給客戶端
- **分離關注點**: 業務錯誤與系統例外分別在不同層級處理
- **載體日誌職責**: 業務邏輯層不記錄錯誤日誌，由 Middleware 記錄
