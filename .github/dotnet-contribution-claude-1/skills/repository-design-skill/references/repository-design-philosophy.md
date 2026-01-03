# Repository Pattern 設計哲學

## 核心原則：需求導向，而非資料表導向

### 錯誤思維：資料表導向

```
資料表: Members, Orders, OrderItems
Repository: MemberRepository, OrderRepository, OrderItemRepository
問題: 業務邏輯分散、跨表操作複雜、難以維護
```

### 正確思維：需求導向

```
業務需求: 會員管理、訂單處理、庫存管理
Repository: MemberRepository, OrderManagementRepository, InventoryRepository
優點: 封裝完整業務邏輯、減少跨層呼叫、更易維護
```

## 設計策略選擇

### 策略 A：資料表導向
- **適用場景**：< 10 個資料表、1-3 人團隊、簡單 CRUD
- **優點**：實作簡單、快速開發
- **缺點**：業務邏輯分散、跨表操作複雜

### 策略 B：需求導向
- **適用場景**：> 10 個資料表、複雜業務邏輯、需要交易
- **優點**：封裝完整、維護性高
- **缺點**：初期設計複雜

### 策略 C：混合模式（本專案採用）
- **簡單主檔**：資料表導向（如 MemberRepository）
- **複雜業務**：需求導向（如 OrderManagementRepository）
- **靈活調整**：根據實際需求演進

## 決策檢查清單

**使用需求導向的判斷標準**：
- [ ] 涉及 3 個以上資料表？
- [ ] 需要交易一致性保證？
- [ ] 業務邏輯複雜，需要多步驟協調？
- [ ] 多個 API 端點共用此業務邏輯？
- [ ] 未來可能擴展更多相關功能？

**如果 2 個以上為「是」，建議使用需求導向**

## 範例對比

### 資料表導向 Repository

```csharp
// ❌ 問題：業務邏輯分散在多個 Repository 和 Handler
public class OrderRepository
{
    public async Task<Result<Order, Failure>> CreateAsync(Order order, CancellationToken ct)
    {
        // 只處理 Orders 表
    }
}

public class OrderItemRepository
{
    public async Task<Result<List<OrderItem>, Failure>> CreateBatchAsync(
        List<OrderItem> items, CancellationToken ct)
    {
        // 只處理 OrderItems 表
    }
}

// Handler 需要協調多個 Repository
public class OrderHandler(
    OrderRepository orderRepo,
    OrderItemRepository itemRepo,
    PaymentRepository paymentRepo)
{
    public async Task<Result<OrderDetail, Failure>> CreateOrderAsync(...)
    {
        // 複雜的跨 Repository 協調邏輯
        await orderRepo.CreateAsync(...);
        await itemRepo.CreateBatchAsync(...);
        await paymentRepo.CreateAsync(...);
    }
}
```

### 需求導向 Repository

```csharp
// ✅ 優勢：封裝完整的業務操作
public class OrderManagementRepository(IDbContextFactory<JobBankDbContext> dbContextFactory)
{
    public async Task<Result<OrderDetail, Failure>> CreateCompleteOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);

        try
        {
            // 1. 建立訂單主檔
            var order = new Order { ... };
            dbContext.Orders.Add(order);

            // 2. 建立訂單明細
            var items = request.Items.Select(i => new OrderItem { ... });
            dbContext.OrderItems.AddRange(items);

            // 3. 建立付款記錄
            var payment = new Payment { ... };
            dbContext.Payments.Add(payment);

            // 4. 更新庫存
            foreach (var item in request.Items)
            {
                var product = await dbContext.Products.FindAsync(item.ProductId);
                product.Stock -= item.Quantity;
            }

            await dbContext.SaveChangesAsync(cancel);
            await transaction.CommitAsync(cancel);

            return Result.Success<OrderDetail, Failure>(orderDetail);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancel);
            return Result.Failure<OrderDetail, Failure>(new Failure { ... });
        }
    }
}

// Handler 變得非常簡潔
public class OrderHandler(OrderManagementRepository orderRepo)
{
    public async Task<Result<OrderDetail, Failure>> CreateOrder(
        CreateOrderRequest request, CancellationToken cancel)
    {
        // 直接呼叫 Repository 的業務方法
        return await orderRepo.CreateCompleteOrderAsync(request, cancel);
    }
}
```

## 命名規範

### 資料表導向
- `{TableName}Repository`
- 範例：MemberRepository, ProductRepository

### 需求導向
- `{BusinessDomain}Repository`
- 範例：OrderManagementRepository, InventoryRepository

## 本專案實作策略

採用**混合模式**：
- **簡單主檔**：MemberRepository（資料表導向）
- **複雜業務**：視需求採用業務導向
- **原則**：從簡單開始，需要時再重構

## 參考資源

- [CLAUDE.md - Repository Pattern 設計哲學](../../../../CLAUDE.md#repository-pattern-設計哲學)
- [MemberRepository 實作範例](../../../../src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs)
