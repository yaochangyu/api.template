---
name: error-handling
description: 錯誤處理與 Result Pattern 技能，協助開發者實作統一的錯誤處理機制，包含 Result Pattern 應用、Failure 物件建立與分層錯誤處理策略。
---

# Error Handling Skill

## 描述
錯誤處理與 Result Pattern 技能，協助開發者實作統一的錯誤處理機制，包含 Result Pattern 應用、Failure 物件建立與分層錯誤處理。

## 職責
- Result Pattern 應用指導
- Failure 物件建立與封裝
- FailureCode 映射至 HTTP 狀態碼
- 分層錯誤處理策略

## 核心原則

### Result Pattern
使用 `CSharpFunctionalExtensions` 套件，統一錯誤處理：
```csharp
public async Task<Result<TSuccess, Failure>> MethodAsync(...)
{
    // 成功路徑
    return Result.Success<TSuccess, Failure>(data);

    // 失敗路徑
    return Result.Failure<TSuccess, Failure>(new Failure { ... });
}
```

### Failure 物件結構
```csharp
public class Failure
{
    public string Code { get; set; }           // 錯誤代碼
    public string Message { get; set; }        // 錯誤訊息
    public string TraceId { get; set; }        // 追蹤 ID
    public Exception? Exception { get; set; }  // 原始例外（不序列化）
    public Dictionary<string, object>? Data { get; set; }  // 額外資料
}
```

### FailureCode 列舉
```csharp
public enum FailureCode
{
    Unauthorized,        // 401
    ValidationError,     // 400
    DuplicateEmail,      // 409
    NotFound,           // 404
    DbError,            // 500
    DbConcurrency,      // 409
    InvalidOperation,   // 400
    Timeout,            // 408
    InternalServerError, // 500
    Unknown             // 500
}
```

## 分層錯誤處理

### Repository 層
- 捕捉資料庫例外（DbUpdateException、DbUpdateConcurrencyException）
- 封裝為 Failure 物件
- 保存原始例外到 Exception 屬性

### Handler 層
- 處理業務邏輯錯誤
- 轉發 Repository 的錯誤
- 不記錄日誌（由 Middleware 處理）

### Controller 層
- 使用 FailureCodeMapper 映射為 HTTP 狀態碼
- 回傳適當的 HTTP 回應

### Middleware 層
- ExceptionHandlingMiddleware 捕捉未處理的系統例外
- 記錄錯誤日誌
- 統一回應格式

## 參考文件
- [Result Pattern 最佳實踐](./references/result-pattern-best-practices.md)

## 範本檔案
- [Failure 範本](./assets/failure-template.cs)
