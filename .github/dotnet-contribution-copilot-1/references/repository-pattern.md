# Repository Pattern 設計指南

> 本文件說明如何在專案中正確設計 Repository，包含資料表導向與需求導向兩種策略

## 核心原則：以需求為導向，而非資料表

### ❌ 錯誤的思維：資料表導向

```
資料表: Members, Orders, OrderItems
Repository: MemberRepository, OrderRepository, OrderItemRepository
問題: 業務邏輯分散、跨表操作複雜、難以維護
```

**問題點**：
1. 業務邏輯分散在多個 Repository 和 Handler
2. Handler 需要協調多個 Repository
3. 交易管理複雜
4. 程式碼重複
5. 難以理解完整的業務流程

### ✅ 正確的思維：需求導向

```
業務需求: 會員管理、訂單處理、庫存管理
Repository: MemberRepository, OrderManagementRepository, InventoryRepository
優點: 封裝完整業務邏輯、減少跨層呼叫、更易維護
```

**優勢**：
1. 封裝完整的業務操作
2. Handler 變得簡潔
3. 交易邊界清晰
4. 易於測試
5. 業務邏輯集中

## 設計策略選擇

### 策略 A：簡單資料表導向

**適用場景**：
- 專案規模小（< 10 個資料表）
- 業務邏輯簡單
- 團隊人數少（1-3 人）
- 快速開發優先

**特徵**：
- 一個 Repository 對應一個資料表
- 提供基本的 CRUD 操作
- 無複雜的業務邏輯

**範例**：
```csharp
public class MemberRepository
{
    public async Task<Result<Member>> GetByIdAsync(Guid id, CancellationToken cancel = default)
    {
        // 簡單的單表查詢
    }
    
    public async Task<Result<Member>> CreateAsync(Member member, CancellationToken cancel = default)
    {
        // 簡單的新增操作
    }
}
```

### 策略 B：業務需求導向

**適用場景**：
- 專案規模中等以上（> 10 個資料表）
- 複雜業務邏輯
- 需要跨表操作
- 長期維護考量

**特徵**：
- Repository 以業務領域命名
- 封裝完整的業務操作
- 處理跨表交易
- 提供高階業務方法

**範例**：
```csharp
public class OrderManagementRepository
{
    // 完整的訂單建立流程（訂單 + 明細 + 付款 + 庫存）
    public async Task<Result<OrderDetail>> CreateCompleteOrderAsync(
        CreateOrderRequest request, 
        CancellationToken cancel = default)
    {
        // 封裝完整業務邏輯
    }
    
    // 取得完整訂單資訊（一次性查詢）
    public async Task<Result<OrderDetail>> GetOrderDetailAsync(
        Guid orderId, 
        CancellationToken cancel = default)
    {
        // 一次查詢取得所有相關資料
    }
}
```

### 策略 C：混合模式（本專案採用）

**適用場景**：
- 核心業務使用需求導向（如訂單處理）
- 簡單主檔使用資料表導向（如會員、產品）
- 根據複雜度靈活調整

**決策原則**：
```
如果滿足以下條件，使用需求導向：
1. 涉及 3 個以上資料表
2. 需要交易一致性
3. 業務邏輯複雜
4. 多個 API 共用
5. 未來可能擴展

否則，使用資料表導向
```

## 實務範例對比

### 範例 1：資料表導向（問題示範）

```csharp
// ❌ 問題：業務邏輯分散在多個 Repository 和 Handler
public class OrderRepository 
{ 
    // 只處理 Orders 表
    public async Task<Result> InsertAsync(Order order) { }
}

public class OrderItemRepository 
{ 
    // 只處理 OrderItems 表
    public async Task<Result> BulkInsertAsync(List<OrderItem> items) { }
}

public class PaymentRepository 
{ 
    // 只處理 Payments 表
    public async Task<Result> InsertAsync(Payment payment) { }
}

// Handler 需要協調多個 Repository
public class OrderHandler(
    OrderRepository orderRepo,
    OrderItemRepository itemRepo,
    PaymentRepository paymentRepo)
{
    public async Task<Result> CreateOrder(CreateOrderRequest request)
    {
        // 複雜的跨 Repository 協調邏輯
        await orderRepo.InsertAsync(...);
        await itemRepo.BulkInsertAsync(...);
        await paymentRepo.InsertAsync(...);
        
        // 問題：
        // 1. 交易管理複雜
        // 2. 錯誤處理分散
        // 3. 業務邏輯外漏到 Handler
    }
}
```

**問題總結**：
- 交易邊界不清晰
- 錯誤處理複雜
- Handler 承擔太多協調責任
- 難以測試完整流程

### 範例 2：需求導向（推薦）

```csharp
// ✅ 優勢：封裝完整的業務操作
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
                TotalAmount = request.Items.Sum(i => i.Price * i.Quantity),
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Orders.Add(order);
            
            // 2. 建立訂單明細
            var items = request.Items.Select(i => new OrderItem 
            { 
                OrderId = order.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            });
            dbContext.OrderItems.AddRange(items);
            
            // 3. 建立付款記錄
            var payment = new Payment 
            { 
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Status = "Pending"
            };
            dbContext.Payments.Add(payment);
            
            // 4. 更新庫存
            foreach (var item in request.Items)
            {
                var product = await dbContext.Products.FindAsync(item.ProductId);
                if (product == null)
                    return Result.Failure<OrderDetail, Failure>(
                        new Failure { Message = $"Product {item.ProductId} not found" });
                
                if (product.Stock < item.Quantity)
                    return Result.Failure<OrderDetail, Failure>(
                        new Failure { Message = $"Insufficient stock for {product.Name}" });
                
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
                new Failure { Message = "Order creation failed", Exception = ex });
        }
    }
    
    public async Task<Result<OrderDetail>> GetOrderDetailAsync(
        Guid orderId, 
        CancellationToken cancel = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
        
        // 一次查詢取得完整訂單資訊（訂單 + 明細 + 付款）
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
            
        if (orderDetail == null)
            return Result.Failure<OrderDetail, Failure>(
                new Failure { Message = "Order not found" });
        
        return Result.Success<OrderDetail, Failure>(orderDetail);
    }
}

// Handler 變得非常簡潔
public class OrderHandler(OrderManagementRepository orderRepo)
{
    public async Task<Result<OrderDetail>> CreateOrder(
        CreateOrderRequest request, 
        CancellationToken cancel)
    {
        // 直接呼叫 Repository 的業務方法
        return await orderRepo.CreateCompleteOrderAsync(request, cancel);
    }
    
    public async Task<Result<OrderDetail>> GetOrderDetail(
        Guid orderId, 
        CancellationToken cancel)
    {
        return await orderRepo.GetOrderDetailAsync(orderId, cancel);
    }
}
```

**優勢總結**：
- ✅ 交易邊界清晰（在 Repository 內）
- ✅ 錯誤處理集中
- ✅ Handler 職責單純
- ✅ 易於測試（模擬 Repository）
- ✅ 業務邏輯封裝完整

## 命名規範建議

### 資料表導向命名
```csharp
{TableName}Repository

範例：
- MemberRepository      // 對應 Members 表
- ProductRepository     // 對應 Products 表
- CategoryRepository    // 對應 Categories 表
```

### 需求導向命名
```csharp
{BusinessDomain}Repository
{AggregateRoot}Repository

範例：
- OrderManagementRepository    // 訂單管理（訂單 + 明細 + 付款）
- InventoryRepository          // 庫存管理（產品 + 庫存 + 調撥）
- ShoppingCartRepository       // 購物車（購物車 + 項目 + 優惠）
- UserAccountRepository        // 使用者帳號（帳號 + 權限 + 登入記錄）
```

## 設計決策檢查清單

### ✅ 需求導向的判斷標準

在設計 Repository 時，問自己：

- [ ] 此業務操作涉及 3 個以上資料表？
- [ ] 操作需要交易一致性保證？
- [ ] 業務邏輯複雜，需要多步驟協調？
- [ ] 多個 API 端點共用此業務邏輯？
- [ ] 未來可能擴展更多相關功能？

**如果以上有 2 個以上為「是」，建議使用需求導向 Repository**

### ❌ 資料表導向的適用場景

- [ ] 僅單一資料表的簡單 CRUD
- [ ] 無複雜業務邏輯
- [ ] 不需要跨表操作
- [ ] 查詢條件簡單明確

**如果以上全部為「是」，可使用資料表導向 Repository**

## 本專案的實作策略

### 混合模式應用

**簡單主檔**（資料表導向）：
- `MemberRepository` - 會員基本資料 CRUD
- `ProductRepository` - 產品基本資料 CRUD
- `CategoryRepository` - 分類基本資料 CRUD

**複雜業務**（需求導向）：
- `OrderManagementRepository` - 訂單處理（訂單 + 明細 + 付款 + 庫存）
- `InventoryRepository` - 庫存管理（產品 + 庫存 + 調撥 + 盤點）
- `ShoppingCartRepository` - 購物車（購物車 + 項目 + 優惠 + 計算）

### 演進策略

**設計初期**：
1. 從簡單的資料表導向開始
2. 快速完成基本功能

**發現問題時**：
1. 業務邏輯分散在多處
2. Handler 過於複雜
3. 難以維護和測試

**重構為需求導向**：
1. 將相關 Repository 合併
2. 封裝完整業務邏輯
3. 簡化 Handler

**重要原則**：
- 不要過度設計
- 根據實際複雜度調整
- 持續重構優化

## 最佳實踐

### 1. 使用 IDbContextFactory
```csharp
public class OrderManagementRepository(
    IDbContextFactory<AppDbContext> dbContextFactory)
{
    // ✅ 每次操作建立新的 DbContext
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
}
```

### 2. 明確的交易邊界
```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);
try
{
    // 業務操作
    await dbContext.SaveChangesAsync(cancel);
    await transaction.CommitAsync(cancel);
}
catch
{
    await transaction.RollbackAsync(cancel);
    throw;
}
```

### 3. 使用 Result Pattern
```csharp
public async Task<Result<OrderDetail>> CreateCompleteOrderAsync(...)
{
    // 成功
    return Result.Success<OrderDetail, Failure>(orderDetail);
    
    // 失敗
    return Result.Failure<OrderDetail, Failure>(new Failure { ... });
}
```

### 4. 查詢最佳化
```csharp
// ✅ 使用 AsNoTracking 提升唯讀查詢效能
var data = await dbContext.Orders
    .AsNoTracking()
    .FirstOrDefaultAsync(cancel);

// ✅ 使用 Include 避免 N+1 問題
var order = await dbContext.Orders
    .Include(o => o.OrderItems)
    .Include(o => o.Payment)
    .FirstOrDefaultAsync(cancel);
```

## 參考實作

**本專案範例**：
- 資料表導向：`src/be/JobBank1111.Job.WebAPI/Member/MemberRepository.cs`

**未來可擴展為需求導向**：
- 當會員功能變複雜時（會員 + 權限 + 登入記錄 + 偏好設定）
- 重構為 `UserAccountRepository`

---

**參考來源**：CLAUDE.md - 專案最佳實踐 - Repository Pattern 設計哲學
