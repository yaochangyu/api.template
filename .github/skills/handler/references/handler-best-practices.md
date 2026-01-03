# Handler 最佳實踐詳解

## Handler 層的職責與定位

Handler 層是業務邏輯的核心，位於 Controller 和 Repository 之間，負責：
- 實作業務規則
- 協調多個 Repository
- 處理業務邏輯錯誤
- 管理交易邊界

## Result Pattern 詳解

### 為什麼使用 Result Pattern？

**問題：傳統例外處理的缺點**
- 業務錯誤和系統錯誤混在一起
- 難以追蹤錯誤流程
- 效能開銷（例外拋出成本高）
- 無法在編譯時檢查錯誤處理

**解決：Result Pattern**
- 明確區分成功與失敗路徑
- 強制處理錯誤（編譯時檢查）
- 更好的效能（無例外拋出）
- 更清晰的業務邏輯

### Result Pattern 實作範例

```csharp
using CSharpFunctionalExtensions;

public class MemberHandler(
    MemberRepository repository,
    IContextGetter<TraceContext> traceContextGetter)
{
    public async Task<Result<MemberResponse, Failure>> CreateMemberAsync(
        CreateMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        try
        {
            // 1. 業務驗證
            var emailExists = await repository.EmailExistsAsync(
                request.Email,
                cancellationToken);

            if (emailExists)
            {
                return Result.Failure<MemberResponse, Failure>(new Failure
                {
                    Code = nameof(FailureCode.DuplicateEmail),
                    Message = "此 Email 已被註冊",
                    TraceId = traceContext.TraceId
                });
            }

            // 2. 建立實體
            var member = new Member
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Name = request.Name,
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow
            };

            // 3. 呼叫 Repository
            var createResult = await repository.CreateAsync(member, cancellationToken);

            // 4. 處理 Repository 的 Result
            return createResult.Match(
                success => Result.Success<MemberResponse, Failure>(new MemberResponse
                {
                    Id = success.Id,
                    Email = success.Email,
                    Name = success.Name,
                    Phone = success.Phone,
                    CreatedAt = success.CreatedAt
                }),
                failure => Result.Failure<MemberResponse, Failure>(failure)
            );
        }
        catch (Exception ex)
        {
            // 5. 捕捉系統層級例外
            return Result.Failure<MemberResponse, Failure>(new Failure
            {
                Code = nameof(FailureCode.InternalServerError),
                Message = ex.Message,
                TraceId = traceContext.TraceId,
                Exception = ex // 必須保存原始例外
            });
        }
    }
}
```

## 交易管理最佳實踐

### 何時需要交易？

**需要交易的場景**：
- 跨多個 Repository 的操作
- 需要保證資料一致性
- 一個操作失敗時需要回滾其他操作

**不需要交易的場景**：
- 單一 Repository 的單一操作（EF Core 自動管理）
- 唯讀查詢操作

### 交易管理範例

```csharp
public class OrderHandler(
    IDbContextFactory<JobBankDbContext> dbContextFactory,
    OrderRepository orderRepository,
    OrderItemRepository itemRepository,
    InventoryRepository inventoryRepository,
    IContextGetter<TraceContext> traceContextGetter)
{
    public async Task<Result<OrderDetail, Failure>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var traceContext = traceContextGetter.Get();

        // 建立 DbContext（使用 Factory 模式）
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        // 開始交易
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. 建立訂單主檔
            var order = new Order
            {
                Id = Guid.NewGuid(),
                MemberId = traceContext.UserId!.Value,
                OrderDate = DateTime.UtcNow,
                TotalAmount = request.Items.Sum(i => i.Price * i.Quantity)
            };

            var orderResult = await orderRepository.CreateAsync(order, cancellationToken);
            if (orderResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<OrderDetail, Failure>(orderResult.Error);
            }

            // 2. 建立訂單明細
            var items = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList();

            var itemsResult = await itemRepository.CreateBatchAsync(items, cancellationToken);
            if (itemsResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<OrderDetail, Failure>(itemsResult.Error);
            }

            // 3. 扣減庫存
            foreach (var item in request.Items)
            {
                var stockResult = await inventoryRepository.ReduceStockAsync(
                    item.ProductId,
                    item.Quantity,
                    cancellationToken);

                if (stockResult.IsFailure)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Failure<OrderDetail, Failure>(stockResult.Error);
                }
            }

            // 4. 提交交易
            await transaction.CommitAsync(cancellationToken);

            return Result.Success<OrderDetail, Failure>(new OrderDetail
            {
                Order = orderResult.Value,
                Items = itemsResult.Value
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<OrderDetail, Failure>(new Failure
            {
                Code = nameof(FailureCode.DbError),
                Message = "訂單建立失敗",
                TraceId = traceContext.TraceId,
                Exception = ex
            });
        }
    }
}
```

## CancellationToken 最佳實踐

### 為什麼需要 CancellationToken？

- 支援請求取消（使用者關閉瀏覽器）
- 支援逾時控制
- 釋放資源，避免浪費

### 使用原則

```csharp
// ✅ 正確：所有非同步方法都支援 CancellationToken
public async Task<Result<T, Failure>> OperationAsync(
    Request request,
    CancellationToken cancellationToken = default)
{
    // 傳遞給所有非同步呼叫
    var result1 = await repository.Method1Async(..., cancellationToken);
    var result2 = await repository.Method2Async(..., cancellationToken);
    var result3 = await externalService.CallAsync(..., cancellationToken);

    return Result.Success<T, Failure>(...);
}

// ❌ 錯誤：忘記傳遞 CancellationToken
public async Task<Result<T, Failure>> OperationAsync(
    Request request,
    CancellationToken cancellationToken = default)
{
    var result = await repository.MethodAsync(...); // 缺少 cancellationToken
    return Result.Success<T, Failure>(...);
}
```

## 不可變物件與 TraceContext

### 使用 TraceContext

```csharp
public class MemberHandler(
    MemberRepository repository,
    IContextGetter<TraceContext> traceContextGetter)
{
    public async Task<Result<MemberResponse, Failure>> GetCurrentMemberAsync(
        CancellationToken cancellationToken = default)
    {
        // 取得當前請求的 TraceContext
        var traceContext = traceContextGetter.Get();

        // 使用 UserId（已在 Middleware 驗證身分並設定）
        var memberId = traceContext.UserId!.Value;

        var result = await repository.GetByIdAsync(memberId, cancellationToken);

        return result.Match(
            success => Result.Success<MemberResponse, Failure>(MapToResponse(success)),
            failure => Result.Failure<MemberResponse, Failure>(failure)
        );
    }
}
```

## 錯誤處理分層策略

### Handler 層的錯誤處理職責

**Handler 應該處理**：
- ✅ 業務邏輯錯誤（Email 重複、庫存不足等）
- ✅ 資料驗證錯誤
- ✅ Repository 回傳的錯誤（轉發）

**Handler 不應該處理**：
- ❌ HTTP 相關錯誤（由 Controller 處理）
- ❌ 系統層級例外的日誌記錄（由 Middleware 處理）

### 錯誤封裝範例

```csharp
// 業務驗證失敗
if (stock < requestQuantity)
{
    return Result.Failure<OrderResponse, Failure>(new Failure
    {
        Code = nameof(FailureCode.InvalidOperation),
        Message = $"庫存不足，可用庫存：{stock}，需求數量：{requestQuantity}",
        TraceId = traceContext.TraceId,
        Data = new Dictionary<string, object>
        {
            ["availableStock"] = stock,
            ["requestedQuantity"] = requestQuantity,
            ["productId"] = productId
        }
    });
}

// 資料庫錯誤
catch (DbUpdateException ex)
{
    return Result.Failure<T, Failure>(new Failure
    {
        Code = nameof(FailureCode.DbError),
        Message = "資料庫操作失敗",
        TraceId = traceContext.TraceId,
        Exception = ex // 保存原始例外供 Middleware 記錄
    });
}

// 併發衝突
catch (DbUpdateConcurrencyException ex)
{
    return Result.Failure<T, Failure>(new Failure
    {
        Code = nameof(FailureCode.DbConcurrency),
        Message = "資料已被其他使用者修改，請重新載入後再試",
        TraceId = traceContext.TraceId,
        Exception = ex
    });
}
```

## 效能最佳化建議

### 1. 避免 N+1 查詢

```csharp
// ❌ 錯誤：N+1 查詢
public async Task<Result<List<OrderWithItems>, Failure>> GetOrdersAsync(
    CancellationToken cancellationToken)
{
    var orders = await repository.GetOrdersAsync(cancellationToken);

    foreach (var order in orders)
    {
        // 每個訂單都查詢一次資料庫
        order.Items = await itemRepository.GetByOrderIdAsync(order.Id, cancellationToken);
    }

    return Result.Success<List<OrderWithItems>, Failure>(orders);
}

// ✅ 正確：使用 Include 或一次查詢
public async Task<Result<List<OrderWithItems>, Failure>> GetOrdersAsync(
    CancellationToken cancellationToken)
{
    // Repository 使用 Include 一次取得所有資料
    var orders = await repository.GetOrdersWithItemsAsync(cancellationToken);
    return Result.Success<List<OrderWithItems>, Failure>(orders);
}
```

### 2. 使用 AsNoTracking for 唯讀查詢

```csharp
// 唯讀查詢不需要追蹤
var members = await repository.GetMembersAsync(
    pageIndex,
    pageSize,
    cancellationToken);
// Repository 內部使用 AsNoTracking()
```

### 3. 批次操作

```csharp
// ✅ 批次插入
var items = request.Items.Select(i => new OrderItem { ... }).ToList();
await repository.CreateBatchAsync(items, cancellationToken);

// ❌ 避免迴圈插入
foreach (var item in request.Items)
{
    await repository.CreateAsync(new OrderItem { ... }, cancellationToken);
}
```

## 測試策略

Handler 的測試應該：
- ✅ 測試業務邏輯正確性
- ✅ 測試錯誤處理路徑
- ✅ 使用真實的 DbContext（Testcontainers）
- ❌ 不要 Mock Repository（除非必要）

## 常見陷阱

### 1. 忘記回滾交易

```csharp
// ❌ 錯誤
try
{
    await operation1();
    if (failed) return Failure; // 忘記回滾
    await operation2();
}

// ✅ 正確
try
{
    await operation1();
    if (failed)
    {
        await transaction.RollbackAsync(cancellationToken);
        return Failure;
    }
    await operation2();
}
```

### 2. 過度設計

```csharp
// ❌ 過度設計：簡單操作不需要複雜的模式
public async Task<Result<Member, Failure>> GetByIdAsync(Guid id, CancellationToken ct)
{
    var validator = new IdValidator();
    var validationResult = validator.Validate(id);
    if (!validationResult.IsValid)
        return Failure;

    var cacheKey = $"member:{id}";
    var cached = await cache.GetAsync(cacheKey);
    if (cached != null) return cached;

    // 實際上簡單查詢即可...
}

// ✅ 簡潔實作
public async Task<Result<Member, Failure>> GetByIdAsync(Guid id, CancellationToken ct)
{
    return await repository.GetByIdAsync(id, ct);
}
```

## 參考資源

- [CSharpFunctionalExtensions 官方文件](https://github.com/vkhorikov/CSharpFunctionalExtensions)
- [Result Pattern 最佳實踐](https://enterprisecraftsmanship.com/posts/functional-c-handling-failures-input-errors/)
- [EF Core 效能最佳化](https://learn.microsoft.com/ef/core/performance/)
