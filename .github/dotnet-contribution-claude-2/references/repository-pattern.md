# Repository Pattern 設計參考文件

## 核心原則：以需求為導向，而非資料表

### ❌ 錯誤的思維：資料表導向

```
資料表結構：
├── Members        (會員資料表)
├── Orders         (訂單資料表)
└── OrderItems     (訂單明細資料表)

Repository 設計：
├── MemberRepository     ← 只處理 Members 表
├── OrderRepository      ← 只處理 Orders 表
└── OrderItemRepository  ← 只處理 OrderItems 表

問題：
❌ 業務邏輯分散在多個 Repository
❌ Handler 需要協調多個 Repository
❌ 跨表操作複雜
❌ 難以維護
❌ 交易管理困難
```

### ✅ 正確的思維：需求導向

```
業務需求：
├── 會員管理       (註冊、登入、個人資料維護)
├── 訂單處理       (建立訂單、查詢訂單、取消訂單)
└── 庫存管理       (更新庫存、查詢庫存)

Repository 設計：
├── MemberRepository            ← 封裝所有會員相關操作
├── OrderManagementRepository   ← 封裝訂單、明細、付款的完整業務流程
└── InventoryRepository         ← 封裝庫存管理相關操作

優點：
✅ 封裝完整業務邏輯
✅ 減少跨層呼叫
✅ 更易維護
✅ 交易管理集中
✅ 符合高內聚低耦合原則
```

## 設計策略選擇

### 策略 A：簡單資料表導向

**適用場景**：
- ✅ 專案規模小（< 10 個資料表）
- ✅ 業務邏輯簡單
- ✅ 團隊人數少（1-3 人）
- ✅ 快速開發優先
- ✅ 主要是單表 CRUD 操作

**範例**：
```csharp
// 單一資料表的簡單操作
public class MemberRepository(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<Result<Member>> GetByIdAsync(Guid id, CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        var member = await dbContext.Members.FindAsync(id);
        return member != null
            ? Result.Success<Member, Failure>(member)
            : Result.Failure<Member, Failure>(new Failure { Code = FailureCode.NotFound });
    }

    public async Task<Result<Member>> CreateAsync(Member member, CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync(cancel);
        return Result.Success<Member, Failure>(member);
    }
}
```

**命名規範**：
- `{TableName}Repository` - 例如：`MemberRepository`, `ProductRepository`

### 策略 B：業務需求導向

**適用場景**：
- ✅ 專案規模中等以上（> 10 個資料表）
- ✅ 複雜業務邏輯
- ✅ 需要跨表操作
- ✅ 長期維護考量
- ✅ 需要交易一致性保證

**範例**：
```csharp
// 封裝完整的業務操作（訂單 + 明細 + 付款）
public class OrderManagementRepository(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<Result<OrderDetail>> CreateCompleteOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);

        try
        {
            // 1. 建立訂單主檔
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderDate = DateTime.UtcNow,
                TotalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice)
            };
            dbContext.Orders.Add(order);

            // 2. 建立訂單明細
            var items = request.Items.Select(i => new OrderItem
            {
                OrderId = order.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            });
            dbContext.OrderItems.AddRange(items);

            // 3. 建立付款記錄
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentDate = DateTime.UtcNow
            };
            dbContext.Payments.Add(payment);

            // 4. 更新庫存
            foreach (var item in request.Items)
            {
                var product = await dbContext.Products.FindAsync(item.ProductId);
                if (product == null)
                    return Result.Failure<OrderDetail, Failure>(
                        new Failure { Code = FailureCode.NotFound, Message = $"Product {item.ProductId} not found" });

                if (product.Stock < item.Quantity)
                    return Result.Failure<OrderDetail, Failure>(
                        new Failure { Code = FailureCode.InsufficientStock, Message = "庫存不足" });

                product.Stock -= item.Quantity;
            }

            await dbContext.SaveChangesAsync(cancel);
            await transaction.CommitAsync(cancel);

            var orderDetail = new OrderDetail
            {
                Order = order,
                Items = items.ToList(),
                Payment = payment
            };

            return Result.Success<OrderDetail, Failure>(orderDetail);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancel);
            return Result.Failure<OrderDetail, Failure>(
                new Failure
                {
                    Code = FailureCode.DbError,
                    Message = ex.Message,
                    Exception = ex
                });
        }
    }

    public async Task<Result<OrderDetail>> GetOrderDetailAsync(
        Guid orderId,
        CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

        // 一次查詢取得完整訂單資訊
        var orderDetail = await dbContext.Orders
            .Where(o => o.Id == orderId)
            .Select(o => new OrderDetail
            {
                Order = o,
                Items = o.OrderItems.ToList(),
                Payment = o.Payment
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancel);

        return orderDetail != null
            ? Result.Success<OrderDetail, Failure>(orderDetail)
            : Result.Failure<OrderDetail, Failure>(
                new Failure { Code = FailureCode.NotFound, Message = "訂單不存在" });
    }
}
```

**命名規範**：
- `{BusinessDomain}Repository` - 例如：`OrderManagementRepository`, `InventoryRepository`
- `{AggregateRoot}Repository` - 例如：`ShoppingCartRepository`, `UserAccountRepository`

### 策略 C：混合模式（本專案採用）

**適用場景**：
- ✅ 實務常見的最佳實踐
- ✅ 根據複雜度靈活調整
- ✅ 平衡開發效率與程式碼品質

**策略**：
- **簡單主檔** → 使用資料表導向（如 `MemberRepository`）
- **複雜業務** → 使用需求導向（如 `OrderManagementRepository`）
- **靈活調整** → 根據實際需求演進

**範例**：
```
專案結構：
├── Member/
│   └── MemberRepository.cs              ← 資料表導向（簡單 CRUD）
├── Order/
│   └── OrderManagementRepository.cs     ← 需求導向（複雜業務）
└── Product/
    └── ProductRepository.cs             ← 資料表導向（簡單 CRUD）
```

## 設計決策檢查清單

在設計 Repository 時，請自問以下問題來決定策略：

### ✅ 需求導向的判斷標準

- [ ] 此業務操作涉及 **3 個以上資料表**？
- [ ] 操作需要 **交易一致性保證**？
- [ ] 業務邏輯複雜，需要 **多步驟協調**？
- [ ] **多個 API 端點** 共用此業務邏輯？
- [ ] 未來可能 **擴展更多相關功能**？

**如果以上有 2 個以上為「是」，建議使用需求導向 Repository**

### ❌ 資料表導向的適用場景

- [ ] 僅 **單一資料表** 的簡單 CRUD
- [ ] **無複雜業務邏輯**
- [ ] **不需要跨表操作**
- [ ] 查詢條件 **簡單明確**
- [ ] 不需要交易管理

**如果以上全部為「是」，可使用資料表導向 Repository**

## 實務對比範例

### 資料表導向的問題

```csharp
// ❌ 問題：業務邏輯分散在 Handler 層
public class OrderHandler(
    OrderRepository orderRepo,
    OrderItemRepository itemRepo,
    PaymentRepository paymentRepo,
    ProductRepository productRepo)
{
    public async Task<Result> CreateOrder(CreateOrderRequest request, CancellationToken cancel)
    {
        // Handler 需要協調多個 Repository
        // 1. 建立訂單
        var orderResult = await orderRepo.CreateAsync(new Order { ... }, cancel);
        if (orderResult.IsFailure) return orderResult;

        // 2. 建立訂單明細
        foreach (var item in request.Items)
        {
            var itemResult = await itemRepo.CreateAsync(new OrderItem { ... }, cancel);
            if (itemResult.IsFailure) return itemResult;
        }

        // 3. 建立付款記錄
        var paymentResult = await paymentRepo.CreateAsync(new Payment { ... }, cancel);
        if (paymentResult.IsFailure) return paymentResult;

        // 4. 更新庫存
        foreach (var item in request.Items)
        {
            var product = await productRepo.GetByIdAsync(item.ProductId, cancel);
            product.Stock -= item.Quantity;
            await productRepo.UpdateAsync(product, cancel);
        }

        // ⚠️ 問題：
        // - 交易管理困難（跨多個 Repository）
        // - 業務邏輯分散
        // - 錯誤處理複雜
        // - 難以維護

        return Result.Success();
    }
}
```

### 需求導向的優勢

```csharp
// ✅ 優勢：業務邏輯集中在 Repository
public class OrderHandler(OrderManagementRepository orderRepo)
{
    public async Task<Result<OrderDetail>> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancel)
    {
        // Handler 變得非常簡潔
        // 直接呼叫 Repository 的業務方法
        return await orderRepo.CreateCompleteOrderAsync(request, cancel);

        // ✅ 優點：
        // - 交易管理集中在 Repository
        // - 業務邏輯完整封裝
        // - 錯誤處理統一
        // - 易於測試與維護
    }
}
```

## 重要原則

### 演進式設計

1. **設計初期**：從簡單的資料表導向開始
2. **發現問題**：當業務邏輯分散、難以維護時
3. **重構改善**：重構為需求導向 Repository
4. **持續優化**：根據實際複雜度調整

### 避免過度設計

⚠️ **不要在專案初期就採用複雜的需求導向設計**

- 先從簡單開始
- 根據實際需求演進
- 重構比一開始就複雜設計更好

### 職責分離

- **Repository 層**：資料存取 + 簡單的資料庫邏輯
- **Handler 層**：複雜的業務規則 + 流程協調

**例外**：需求導向 Repository 可以包含完整的業務流程（如訂單建立）

## 實作參考

### 資料表導向範例
📝 [MemberRepository.cs](../../src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs)

### 程式碼範本
📝 [repository-template.cs](../assets/repository-template.cs)

## 參考資源

- 📚 [CLAUDE.md](../../../CLAUDE.md) - 完整專案指導文件
- 📝 [架構設計](./architecture.md) - 分層架構說明
- 📝 [EF Core 最佳實踐](./ef-core-best-practices.md) - DbContextFactory 與查詢最佳化
- 📝 [錯誤處理](./error-handling.md) - Result Pattern 使用方式
