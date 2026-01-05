# Result Pattern 最佳實踐指南

## 什麼是 Result Pattern?

Result Pattern 是一種錯誤處理模式，透過回傳 Result 物件來明確表示操作的成功或失敗，而不是使用例外處理。這種模式讓錯誤處理更加明確、可預測，並且提升程式碼的可讀性。

### 核心概念

- **明確的錯誤處理**：透過型別系統強制處理錯誤情境
- **避免例外濫用**：保留例外處理給真正的例外情況
- **可組合性**：透過鏈式呼叫 (Method Chaining) 組合多個操作
- **類型安全**：編譯期間即可發現潛在的錯誤處理遺漏

### 適用情境

✅ **適合使用 Result Pattern 的場景：**
- 驗證邏輯 (Validation)
- 業務規則檢查
- 外部 API 呼叫
- 資料庫操作
- 可預期的錯誤情況

❌ **不適合使用 Result Pattern 的場景：**
- 系統層級錯誤 (OutOfMemoryException)
- 程式設計錯誤 (NullReferenceException, ArgumentException)
- 無法恢復的錯誤
- 真正的例外情況

參考資料：[Exceptions for Flow Control by Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/exceptions-for-flow-control/)

---

## .NET 生態系統中的 Result Pattern 套件

### 1. FluentResults

**NuGet 套件：** `FluentResults`  
**Repository:** https://github.com/altmann/fluentresults  
**特色：**
- 流暢的 API 設計
- 支援多個錯誤訊息
- 階層式錯誤鏈
- 自訂錯誤類型
- 豐富的 Metadata 支援

#### 最佳實踐

##### 1.1 建立 Result 物件

```csharp
// 成功結果
Result successResult = Result.Ok();
Result<User> userResult = Result.Ok(user);

// 失敗結果 - 使用字串訊息
Result errorResult = Result.Fail("User not found");

// 失敗結果 - 使用 Error 物件
Result errorResult = Result.Fail(new Error("User not found"));

// 失敗結果 - 使用自訂錯誤類型
Result errorResult = Result.Fail(new UserNotFoundError(userId));

// 失敗結果 - 多個錯誤
Result errorResult = Result.Fail(new List<IError> 
{ 
    new Error("Validation error 1"), 
    new Error("Validation error 2") 
});
```

##### 1.2 處理 Result 物件

```csharp
Result<int> result = GetUserAge(userId);

// 檢查狀態
if (result.IsFailed)
{
    // 取得所有錯誤
    IEnumerable<IError> errors = result.Errors;
    
    // 使用 ValueOrDefault 避免例外
    var age = result.ValueOrDefault; // 回傳 0
    return;
}

// 成功時取得值
var age = result.Value; // 安全取得值
```

##### 1.3 鏈式呼叫錯誤

```csharp
var result = Result.Fail("Primary error")
    .WithError("Additional error 1")
    .WithError("Additional error 2")
    .WithSuccess("Partial success message");
```

##### 1.4 Pattern Matching

```csharp
var result = Result.Fail<int>("Error occurred");

var outcome = result switch
{
    { IsFailed: true } => $"Failed: {string.Join(", ", result.Errors)}",
    { IsSuccess: true } => $"Success: {result.Value}",
    _ => "Unknown state"
};
```

##### 1.5 自訂錯誤類型

```csharp
public class UserNotFoundError : Error
{
    public UserNotFoundError(string userId) 
        : base($"User with ID '{userId}' not found")
    {
        Metadata.Add("UserId", userId);
        Metadata.Add("ErrorCode", "USER_NOT_FOUND");
    }
}

// 使用
Result result = Result.Fail(new UserNotFoundError("user123"));
```

---

### 2. ErrorOr

**NuGet 套件：** `ErrorOr`  
**Repository:** https://github.com/amantinband/error-or  
**特色：**
- 輕量級、簡單易用
- Discriminated Union 設計
- 強大的函數式方法鏈
- 型別安全

#### 最佳實踐

##### 2.1 建立 ErrorOr 物件

```csharp
// 成功結果
ErrorOr<User> result = user;

// 單一錯誤
ErrorOr<User> result = Error.NotFound(description: "User not found");
ErrorOr<User> result = Error.Validation(description: "Invalid email format");

// 多個錯誤
ErrorOr<User> result = new List<Error> 
{ 
    Error.Validation(description: "Email is required"),
    Error.Validation(description: "Password is too short")
};

// 預定義的錯誤類型
public static class UserErrors
{
    public static Error NotFound => Error.NotFound(
        code: "User.NotFound",
        description: "User was not found");
        
    public static Error UnderAge => Error.Validation(
        code: "User.UnderAge",
        description: "User must be 18 or older");
}
```

##### 2.2 函數式方法鏈 (Method Chaining)

```csharp
// 成功流程
ErrorOr<string> result = await "2".ToErrorOr()
    .Then(int.Parse)                    // 轉換為整數
    .FailIf(val => val > 2,            // 條件驗證
        Error.Validation(description: $"{val} is too big"))
    .ThenDoAsync(Task.Delay)           // 執行非同步副作用
    .ThenDo(val => Console.WriteLine($"Waited {val}ms"))  // 執行同步副作用
    .ThenAsync(val => Task.FromResult(val * 2))          // 非同步轉換
    .Then(val => $"Result: {val}")     // 同步轉換
    .MatchFirst(
        value => value,
        firstError => $"Error: {firstError.Description}");

// 失敗流程
ErrorOr<string> result = await "5".ToErrorOr()
    .Then(int.Parse)                    // 5
    .FailIf(val => val > 2,            // 驗證失敗
        Error.Validation(description: $"{val} is too big"))
    .ThenDoAsync(Task.Delay)           // 跳過
    .Then(val => val * 2)              // 跳過
    .Else(errors => Error.Unexpected(description: "Fallback error"))
    .MatchFirst(
        value => value,
        firstError => $"Error: {firstError.Description}");
```

##### 2.3 實際應用範例

```csharp
public async Task<IActionResult> IncrementUserAge(string id)
{
    return await _userRepository.GetByIdAsync(id)
        .Then(user => user.IncrementAge()
            .Then(success => user)
            .Else(errors => Error.Unexpected("Age increment failed")))
        .FailIf(user => !user.IsOverAge(18), UserErrors.UnderAge)
        .ThenDo(user => _logger.LogInformation(
            $"User {user.Id} age incremented to {user.Age}"))
        .ThenAsync(user => _userRepository.UpdateAsync(user))
        .Match(
            _ => NoContent(),
            errors => errors.ToActionResult());
}
```

##### 2.4 條件式失敗 (FailIf)

```csharp
ErrorOr<int> result = GetAge()
    .FailIf(age => age < 18, Error.Validation("Must be 18 or older"))
    .FailIf(age => age > 120, Error.Validation("Invalid age"))
    .Then(age => age * 2);  // 只有在所有驗證通過後才執行
```

##### 2.5 Switch 與 Match

```csharp
// Switch - 轉換結果
var message = result.Switch(
    value => $"Success: {value}",
    errors => $"Failed with {errors.Count} errors");

// Match - 回傳不同類型
var actionResult = result.Match(
    value => Ok(value),
    errors => BadRequest(errors));

// MatchFirst - 只處理第一個錯誤
var message = result.MatchFirst(
    value => $"Success: {value}",
    firstError => $"Error: {firstError.Description}");
```

---

### 3. LanguageExt

**NuGet 套件：** `LanguageExt.Core`  
**Repository:** https://github.com/louthy/language-ext  
**特色：**
- 完整的函數式程式設計函式庫
- Either、Option、Validation 等多種 Monad
- 不可變資料結構
- 適合進階函數式程式設計

#### 最佳實踐

##### 3.1 Either Monad

```csharp
// 定義 Either
Either<Error, User> result = user;  // Right (成功)
Either<Error, User> result = error; // Left (失敗)

// 建立 Left 或 Right
Either<string, int> success = Prelude.Right<string, int>(42);
Either<string, int> failure = Prelude.Left<string, int>("Error occurred");
```

##### 3.2 Pattern Matching

```csharp
var message = result.Match(
    Left: error => $"Error: {error}",
    Right: user => $"User: {user.Name}");
```

##### 3.3 Bind 與 Map

```csharp
Either<Error, int> result = GetUser(userId)
    .Map(user => user.Age)                    // 轉換 Right 值
    .Bind(age => ValidateAge(age))            // 鏈接可能失敗的操作
    .Map(age => age * 2);
```

##### 3.4 使用 Validation

```csharp
using LanguageExt;
using static LanguageExt.Prelude;

// Validation 可以累積多個錯誤
Validation<Error, User> ValidateUser(UserInput input)
{
    return (ValidateEmail(input.Email), ValidateName(input.Name))
        .Apply((email, name) => new User(email, name));
}

Validation<Error, string> ValidateEmail(string email) =>
    string.IsNullOrEmpty(email)
        ? Fail<Error, string>(Error.New("Email is required"))
        : Success<Error, string>(email);

Validation<Error, string> ValidateName(string name) =>
    string.IsNullOrEmpty(name)
        ? Fail<Error, string>(Error.New("Name is required"))
        : Success<Error, string>(name);
```

---

### 4. OneOf

**NuGet 套件：** `OneOf`  
**Repository:** https://github.com/mcintyre321/OneOf  
**特色：**
- Discriminated Union 實作
- 支援最多 9 種類型
- 編譯期型別安全
- 輕量級、零依賴

#### 最佳實踐

##### 4.1 基本使用

```csharp
using OneOf;

// 定義可能的回傳類型
OneOf<User, NotFoundError, ValidationError> result = GetUser(userId);

// Pattern Matching
var message = result.Match(
    user => $"Found: {user.Name}",
    notFound => $"Error: {notFound.Message}",
    validation => $"Validation failed: {validation.Errors}");
```

##### 4.2 建立專用的 Result 類型

```csharp
public class Result<TSuccess, TError> : OneOfBase<TSuccess, TError>
{
    public Result(OneOf<TSuccess, TError> input) : base(input) { }
    
    public static implicit operator Result<TSuccess, TError>(TSuccess success) 
        => new(success);
    
    public static implicit operator Result<TSuccess, TError>(TError error) 
        => new(error);
}

// 使用
Result<User, Error> result = user;  // 隱式轉換
Result<User, Error> result = error; // 隱式轉換
```

---

### 5. CSharpFunctionalExtensions

**NuGet 套件：** `CSharpFunctionalExtensions`  
**Repository:** https://github.com/vkhorikov/CSharpFunctionalExtensions  
**特色：**
- Vladimir Khorikov 開發 (Domain-Driven Design 專家)
- Result、Maybe、ValueObject 支援
- Railway Oriented Programming
- DDD 友善

#### 最佳實踐

##### 5.1 基本 Result 使用

```csharp
using CSharpFunctionalExtensions;

// 成功
Result<User> result = Result.Success(user);

// 失敗
Result<User> result = Result.Failure<User>("User not found");

// 無值的 Result
Result result = Result.Success();
Result result = Result.Failure("Operation failed");
```

##### 5.2 Railway Oriented Programming

```csharp
public Result<User> UpdateUser(string userId, UserUpdateDto dto)
{
    return GetUser(userId)
        .Ensure(user => user.IsActive, "User is not active")
        .Bind(user => user.UpdateEmail(dto.Email))
        .Bind(user => user.UpdateName(dto.Name))
        .Tap(user => _logger.LogInformation($"Updated user {user.Id}"))
        .Finally(result => result.IsSuccess 
            ? _repository.Save(result.Value) 
            : Result.Failure<User>(result.Error));
}
```

##### 5.3 Combine 與 Ensure

```csharp
// Combine - 組合多個結果
Result finalResult = Result.Combine(
    ValidateEmail(email),
    ValidatePassword(password),
    ValidateAge(age));

// Ensure - 條件驗證
Result<User> result = GetUser(userId)
    .Ensure(user => user.Age >= 18, "User must be 18 or older")
    .Ensure(user => user.IsActive, "User must be active");
```

---

## 套件比較與選擇建議

| 套件 | 複雜度 | 學習曲線 | 函數式特性 | 適用場景 |
|------|--------|----------|------------|----------|
| **FluentResults** | 低 | 簡單 | 中等 | 一般專案、快速上手 |
| **ErrorOr** | 低 | 簡單 | 中等 | RESTful API、Clean Architecture |
| **CSharpFunctionalExtensions** | 中 | 中等 | 高 | DDD、Enterprise 應用 |
| **LanguageExt** | 高 | 陡峭 | 非常高 | 進階函數式程式設計 |
| **OneOf** | 低 | 簡單 | 低 | 需要 Discriminated Union |

### 選擇建議

1. **新手或一般專案：** FluentResults 或 ErrorOr
2. **DDD 或 Enterprise 專案：** CSharpFunctionalExtensions
3. **函數式程式設計愛好者：** LanguageExt
4. **需要多種類型判別：** OneOf
5. **Clean Architecture / Minimal API：** ErrorOr

---

## 通用最佳實踐

### 1. 定義領域特定的錯誤類型

```csharp
public static class UserErrors
{
    public static Error NotFound(string userId) => Error.NotFound(
        code: "User.NotFound",
        description: $"User '{userId}' was not found");

    public static Error InvalidEmail => Error.Validation(
        code: "User.InvalidEmail",
        description: "Email format is invalid");
        
    public static Error UnderAge => Error.Validation(
        code: "User.UnderAge",
        description: "User must be 18 years or older");
}
```

### 2. 在邊界層轉換 Result 為 HTTP Response

```csharp
// ASP.NET Core Controller
public async Task<IActionResult> GetUser(string id)
{
    var result = await _userService.GetUserAsync(id);
    
    return result.Match(
        user => Ok(user),
        errors => errors.First() switch
        {
            { Type: ErrorType.NotFound } => NotFound(errors),
            { Type: ErrorType.Validation } => BadRequest(errors),
            _ => StatusCode(500, errors)
        });
}
```

### 3. Repository 層回傳 Result

```csharp
public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(string id);
    Task<Result<User>> CreateAsync(User user);
    Task<Result<User>> UpdateAsync(User user);
    Task<Result> DeleteAsync(string id);
}
```

### 4. Service 層組合業務邏輯

```csharp
public class UserService
{
    public async Task<Result<User>> UpdateUserEmailAsync(string userId, string newEmail)
    {
        return await _repository.GetByIdAsync(userId)
            .Bind(user => ValidateEmail(newEmail)
                .Bind(email => user.UpdateEmail(email)))
            .Bind(user => _repository.UpdateAsync(user))
            .TapAsync(user => _eventPublisher.PublishAsync(
                new UserEmailUpdatedEvent(user.Id, newEmail)));
    }
}
```

### 5. 避免混合使用 Result 與 Exception

❌ **不好的做法：**
```csharp
public Result<User> GetUser(string id)
{
    if (string.IsNullOrEmpty(id))
        throw new ArgumentNullException(nameof(id)); // 混用例外
        
    var user = _repository.Find(id);
    return user == null 
        ? Result.Fail<User>("Not found") 
        : Result.Ok(user);
}
```

✅ **好的做法：**
```csharp
public Result<User> GetUser(string id)
{
    // 前置條件檢查仍使用例外
    if (string.IsNullOrEmpty(id))
        throw new ArgumentNullException(nameof(id));
        
    var user = _repository.Find(id);
    return user == null 
        ? Result.Fail<User>("User not found") 
        : Result.Ok(user);
}
```

### 6. 使用擴充方法增強可讀性

```csharp
public static class ResultExtensions
{
    public static async Task<Result<T>> ToResultAsync<T>(
        this Task<T?> task, 
        string errorMessage) where T : class
    {
        var value = await task;
        return value == null 
            ? Result.Fail<T>(errorMessage) 
            : Result.Ok(value);
    }
    
    public static Result<T> ToResult<T>(
        this T? value, 
        string errorMessage) where T : class
    {
        return value == null 
            ? Result.Fail<T>(errorMessage) 
            : Result.Ok(value);
    }
}

// 使用
var result = await _repository.FindAsync(id)
    .ToResultAsync("User not found");
```

---

## 結論

Result Pattern 是現代 .NET 開發中處理錯誤的強大工具。選擇適合的套件並遵循最佳實踐，可以讓你的程式碼更加健壯、可維護且易於理解。

**關鍵要點：**
- ✅ 使用 Result Pattern 處理可預期的錯誤
- ✅ 保留例外處理給真正的例外情況
- ✅ 使用型別系統強制錯誤處理
- ✅ 選擇符合團隊技能與專案需求的套件
- ✅ 在架構邊界明確轉換 Result 為其他形式
