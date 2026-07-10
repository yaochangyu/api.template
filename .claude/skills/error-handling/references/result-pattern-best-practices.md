# Result Pattern 最佳實踐

## 核心概念

Result Pattern 使用 `Result<TSuccess, TFailure>` 取代異常拋出，提供：
- ✅ 強型別錯誤處理
- ✅ 函式簽名明確表達可能失敗
- ✅ 編譯時檢查（不會遺漏錯誤處理）
- ✅ 更好的 composability

## 分層應用

### Repository 層（資料存取）

**職責**：
- 捕捉資料庫異常
- 轉換為 Failure 物件
- 保存原始異常供追蹤

**實作要點**：
```csharp
// 1. 指定異常類型（不要捕捉所有 Exception）
try { ... }
catch (DbUpdateException ex)        // ✅ 預期異常
catch (DbUpdateConcurrencyException ex)  // ✅ 預期異常
catch (OperationCanceledException)  // ✅ 取消操作

// 2. 轉換為 Failure 並保存原始異常
new Failure 
{ 
    Code = nameof(FailureCode.DbError),
    Message = "Database error",
    Exception = ex  // ⭐ 必須保存
}

// 3. 包含 TraceId（由 Middleware 注入）
new Failure 
{ 
    TraceId = traceContext.TraceId,  // 用於追蹤
    ...
}
```

### Handler 層（業務邏輯）

**職責**：
- 處理業務邏輯錯誤（驗證、業務規則）
- 協調多個 Repository 呼叫
- 轉發 Repository 錯誤
- **不記錄日誌**（由 Middleware 處理）

**實作要點**：
```csharp
// 1. 驗證請求
var validationResult = await validator.ValidateAsync(request);
if (!validationResult.IsValid)
    return Result.Failure<T, Failure>(new Failure 
    { 
        Code = nameof(FailureCode.ValidationError),
        Message = "Validation failed"
    });

// 2. 呼叫 Repository 並轉發結果
var dbResult = await repo.InsertAsync(entity);
if (dbResult.IsFailure)
    return Result.Failure<T, Failure>(dbResult.Error);  // 直接轉發

// 3. 檢查業務規則
if (entity.Balance < 0)
    return Result.Failure<T, Failure>(new Failure 
    { 
        Code = nameof(FailureCode.InvalidOperation),
        Message = "Insufficient balance"
    });
```

### Controller 層（HTTP 對應）

**職責**：
- 呼叫 Handler
- 使用 FailureCodeMapper 轉換為 HTTP 狀態碼
- 回傳適當的 HTTP 回應

**實作要點**：
```csharp
// 1. 呼叫 Handler
var result = await handler.CreateAsync(request, cancel);

// 2. 檢查結果
if (result.IsSuccess)
    return Created(..., result.Value);

// 3. 轉換為 HTTP 回應
var (statusCode, errorResponse) = mapper.Map(result.Error);
return StatusCode(statusCode, errorResponse);
```

## 常見錯誤模式

### ❌ 錯誤 1：在 Handler 記錄日誌
Handler 中記錄會導致重複日誌（Middleware 已記錄）。將日誌集中在 Middleware。

### ❌ 錯誤 2：丟棄原始異常
```csharp
new Failure { Code = "Error" }  // ❌ 沒有 Exception
```
必須保存 `Exception` 屬性供除錯與追蹤。

### ❌ 錯誤 3：捕捉所有 Exception
```csharp
catch (Exception ex) { ... }  // ❌ 太寬泛
```
只捕捉預期異常（DbUpdateException、DbUpdateConcurrencyException）。

### ❌ 錯誤 4：在業務邏輯層拋出異常
```csharp
throw new InvalidOperationException("...");  // ❌
```
使用 Result Pattern 回傳錯誤。

## 型別安全檢查清單

- [ ] 所有可能失敗的方法返回 `Result<T, Failure>`
- [ ] 異常都被捕捉且保存在 `Failure.Exception`
- [ ] Repository/Handler 層不記錄日誌
- [ ] Controller 層使用 FailureCodeMapper
- [ ] 所有 Failure 都包含 TraceId
