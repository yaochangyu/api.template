# EF Core 最佳實踐參考文件

## DbContextFactory 模式

### 為什麼使用 DbContextFactory？

**❌ 傳統方式的問題（直接注入 DbContext）**：
```csharp
// ❌ 不推薦：直接注入 DbContext
public class MemberRepository(AppDbContext dbContext)
{
    public async Task<Member> GetAsync(Guid id)
    {
        return await dbContext.Members.FindAsync(id);
    }
}

// 問題：
// 1. DbContext 生命週期問題（Scoped vs Singleton）
// 2. 長時間持有連線
// 3. 無法控制 DbContext 的建立與釋放
// 4. 可能導致記憶體洩漏
```

**✅ 推薦方式（使用 DbContextFactory）**：
```csharp
// ✅ 推薦：使用 IDbContextFactory<T>
public class MemberRepository(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<Result<Member, Failure>> GetAsync(
        Guid id,
        CancellationToken cancel = default)
    {
        // 每次操作建立新的 DbContext
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

        var member = await dbContext.Members.FindAsync(new object[] { id }, cancel);

        return member != null
            ? Result.Success<Member, Failure>(member)
            : Result.Failure<Member, Failure>(new Failure
            {
                Code = FailureCode.NotFound,
                Message = $"會員 {id} 不存在"
            });
    }
    // DbContext 在方法結束後自動釋放
}
```

**優點**：
- ✅ 明確控制 DbContext 生命週期
- ✅ 避免長時間持有連線
- ✅ 防止記憶體洩漏
- ✅ 支援並行操作
- ✅ 更好的測試性

### DbContextFactory 註冊

```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            // 啟用連線彈性（自動重試）
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);

            // 設定命令逾時
            sqlOptions.CommandTimeout(30);
        });

    // 開發環境啟用敏感資料記錄
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
```

## 非同步程式設計最佳實踐

### 核心原則

1. **所有 I/O 操作都使用 async/await**
2. **所有非同步方法都支援 CancellationToken**
3. **避免使用 `.Result` 或 `.Wait()`（死鎖風險）**

### 正確的非同步操作

```csharp
// ✅ 正確：使用 async/await + CancellationToken
public async Task<Result<Member, Failure>> GetAsync(
    Guid id,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // ✅ 使用 FindAsync 並傳遞 CancellationToken
    var member = await dbContext.Members.FindAsync(new object[] { id }, cancel);

    return member != null
        ? Result.Success<Member, Failure>(member)
        : Result.Failure<Member, Failure>(new Failure { Code = FailureCode.NotFound });
}

// ✅ 查詢操作
public async Task<Result<List<Member>, Failure>> GetAllAsync(
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    var members = await dbContext.Members
        .AsNoTracking()  // ✅ 唯讀查詢使用 AsNoTracking()
        .ToListAsync(cancel);  // ✅ 傳遞 CancellationToken

    return Result.Success<List<Member>, Failure>(members);
}
```

### ❌ 常見錯誤

```csharp
// ❌ 錯誤 1：使用 .Result（死鎖風險）
public Member Get(Guid id)
{
    var member = dbContext.Members.FindAsync(id).Result;  // ❌ 不要使用 .Result
    return member;
}

// ❌ 錯誤 2：使用 .Wait()（死鎖風險）
public Member Get(Guid id)
{
    var task = dbContext.Members.FindAsync(id);
    task.Wait();  // ❌ 不要使用 .Wait()
    return task.Result;
}

// ❌ 錯誤 3：忘記傳遞 CancellationToken
public async Task<Member> GetAsync(Guid id, CancellationToken cancel)
{
    var member = await dbContext.Members.FindAsync(id);  // ❌ 沒有傳遞 cancel
    return member;
}

// ❌ 錯誤 4：不必要的 ToListAsync().Result
public List<Member> GetAll()
{
    return dbContext.Members.ToListAsync().Result;  // ❌ 同步方法中呼叫非同步
}
```

## 查詢最佳化

### AsNoTracking() 提升效能

```csharp
// ✅ 唯讀查詢：使用 AsNoTracking()
public async Task<Result<List<Member>, Failure>> GetMembersAsync(
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    var members = await dbContext.Members
        .AsNoTracking()  // ✅ 不需要追蹤變更
        .Where(m => m.IsActive)
        .OrderBy(m => m.Name)
        .ToListAsync(cancel);

    return Result.Success<List<Member>, Failure>(members);
}

// ✅ 需要更新的查詢：不使用 AsNoTracking()
public async Task<Result<Member, Failure>> UpdateAsync(
    Member member,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // ❌ 不要使用 AsNoTracking()（需要追蹤變更）
    var existing = await dbContext.Members.FindAsync(new object[] { member.Id }, cancel);

    if (existing == null)
        return Result.Failure<Member, Failure>(new Failure { Code = FailureCode.NotFound });

    // 更新屬性
    existing.Name = member.Name;
    existing.Email = member.Email;

    await dbContext.SaveChangesAsync(cancel);

    return Result.Success<Member, Failure>(existing);
}
```

**AsNoTracking() 的效能影響**：
- ✅ 減少記憶體使用（不追蹤實體）
- ✅ 提升查詢效能（少了變更偵測）
- ✅ 適用於唯讀查詢

### 避免 N+1 查詢問題

```csharp
// ❌ 錯誤：N+1 查詢問題
public async Task<List<OrderWithItems>> GetOrdersAsync()
{
    var orders = await dbContext.Orders.ToListAsync();

    // ❌ 每個訂單都會執行一次查詢（N+1 問題）
    foreach (var order in orders)
    {
        order.Items = await dbContext.OrderItems
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();
    }

    return orders;
}

// ✅ 正確：使用 Include 一次查詢
public async Task<Result<List<Order>, Failure>> GetOrdersAsync(
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    var orders = await dbContext.Orders
        .Include(o => o.Items)  // ✅ 使用 Include 載入相關資料
        .AsNoTracking()
        .ToListAsync(cancel);

    return Result.Success<List<Order>, Failure>(orders);
}

// ✅ 更好：使用 Select 投影（只載入需要的欄位）
public async Task<Result<List<OrderSummary>, Failure>> GetOrderSummariesAsync(
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    var summaries = await dbContext.Orders
        .Select(o => new OrderSummary
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            ItemCount = o.Items.Count  // ✅ 使用 Select 避免載入完整 Items
        })
        .AsNoTracking()
        .ToListAsync(cancel);

    return Result.Success<List<OrderSummary>, Failure>(summaries);
}
```

### 分頁查詢

```csharp
public async Task<Result<PagedResult<Member>, Failure>> GetPagedMembersAsync(
    int pageIndex,
    int pageSize,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // 驗證參數
    if (pageIndex < 0 || pageSize <= 0 || pageSize > 100)
    {
        return Result.Failure<PagedResult<Member>, Failure>(new Failure
        {
            Code = FailureCode.ValidationError,
            Message = "分頁參數不正確"
        });
    }

    // 計算總筆數
    var totalCount = await dbContext.Members.CountAsync(cancel);

    // 取得分頁資料
    var members = await dbContext.Members
        .AsNoTracking()
        .OrderBy(m => m.Name)
        .Skip(pageIndex * pageSize)  // ✅ 分頁：跳過
        .Take(pageSize)               // ✅ 分頁：取得
        .ToListAsync(cancel);

    var result = new PagedResult<Member>
    {
        Items = members,
        TotalCount = totalCount,
        PageIndex = pageIndex,
        PageSize = pageSize
    };

    return Result.Success<PagedResult<Member>, Failure>(result);
}
```

## 交易管理

### 明確的交易處理

```csharp
public async Task<Result<Order, Failure>> CreateOrderAsync(
    CreateOrderRequest request,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // ✅ 開始交易
    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancel);

    try
    {
        // 1. 建立訂單
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            TotalAmount = request.TotalAmount
        };
        dbContext.Orders.Add(order);

        // 2. 建立訂單明細
        foreach (var item in request.Items)
        {
            dbContext.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });

            // 3. 更新庫存
            var product = await dbContext.Products.FindAsync(new object[] { item.ProductId }, cancel);
            if (product == null)
            {
                return Result.Failure<Order, Failure>(new Failure
                {
                    Code = FailureCode.NotFound,
                    Message = $"產品 {item.ProductId} 不存在"
                });
            }

            product.Stock -= item.Quantity;
        }

        // 儲存變更
        await dbContext.SaveChangesAsync(cancel);

        // ✅ 提交交易
        await transaction.CommitAsync(cancel);

        return Result.Success<Order, Failure>(order);
    }
    catch (Exception ex)
    {
        // ✅ 回滾交易
        await transaction.RollbackAsync(cancel);

        return Result.Failure<Order, Failure>(new Failure
        {
            Code = FailureCode.DbError,
            Message = ex.Message,
            Exception = ex
        });
    }
}
```

## 批次操作

### 使用 AddRange / UpdateRange / RemoveRange

```csharp
// ✅ 批次新增
public async Task<Result<List<Member>, Failure>> CreateBatchAsync(
    List<Member> members,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // ✅ 使用 AddRange 批次新增
    dbContext.Members.AddRange(members);

    await dbContext.SaveChangesAsync(cancel);

    return Result.Success<List<Member>, Failure>(members);
}

// ✅ 批次刪除
public async Task<Result> DeleteBatchAsync(
    List<Guid> ids,
    CancellationToken cancel = default)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);

    // 查詢要刪除的實體
    var members = await dbContext.Members
        .Where(m => ids.Contains(m.Id))
        .ToListAsync(cancel);

    // ✅ 使用 RemoveRange 批次刪除
    dbContext.Members.RemoveRange(members);

    await dbContext.SaveChangesAsync(cancel);

    return Result.Success();
}
```

## 連線彈性（Retry Policy）

### 自動重試機制

```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            // ✅ 啟用連線彈性（自動重試）
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,                     // 最多重試 3 次
                maxRetryDelay: TimeSpan.FromSeconds(5), // 最大延遲 5 秒
                errorNumbersToAdd: null                // 可指定特定錯誤碼才重試
            );
        });
});
```

**重試策略說明**：
- ✅ 自動處理暫時性錯誤（如網路中斷、連線逾時）
- ✅ 指數退避（exponential backoff）
- ✅ 適用於雲端環境

## EF Core Migration

### 命令列操作（透過 Taskfile）

```bash
# ⚠️ 重要：必須透過 Taskfile 執行，不應直接執行 dotnet ef

# 建立新的 Migration
task ef-migration-add NAME=AddMemberTable

# 更新資料庫
task ef-database-update

# 移除最後一個 Migration
task ef-migration-remove

# 查看 Migration 狀態
task ef-migration-list
```

### Taskfile 配置

```yaml
tasks:
  ef-migration-add:
    desc: 建立新的 EF Core Migration
    cmds:
      - dotnet ef migrations add {{.NAME}} --project src/be/JobBank1111.Job.DB --startup-project src/be/JobBank1111.Job.WebAPI

  ef-database-update:
    desc: 更新資料庫至最新的 Migration
    cmds:
      - dotnet ef database update --project src/be/JobBank1111.Job.DB --startup-project src/be/JobBank1111.Job.WebAPI
```

### Code First vs Database First

**Code First（推薦）**：
- ✅ 先定義 C# 類別，再產生資料庫結構
- ✅ 版本控制友善（Migration 檔案）
- ✅ 適合新專案

**Database First**：
- ✅ 從現有資料庫反向工程產生實體
- ✅ 適合既有資料庫專案
- ✅ 使用 `task ef-codegen` 執行反向工程

## 常見陷阱與錯誤

### ❌ 陷阱 1：追蹤狀態混淆

```csharp
// ❌ 錯誤：嘗試更新 AsNoTracking() 查詢的實體
var member = await dbContext.Members
    .AsNoTracking()  // ❌ 不追蹤
    .FirstOrDefaultAsync(m => m.Id == id);

member.Name = "New Name";
await dbContext.SaveChangesAsync();  // ❌ 不會儲存（未追蹤）
```

### ❌ 陷阱 2：重複使用 DbContext

```csharp
// ❌ 錯誤：重複使用同一個 DbContext
private readonly AppDbContext _dbContext;

public MemberRepository(IDbContextFactory<AppDbContext> factory)
{
    _dbContext = factory.CreateDbContext();  // ❌ 不應在建構函式建立
}

// ✅ 正確：每次操作建立新的 DbContext
public async Task<Member> GetAsync(Guid id)
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancel);
    // ...
}
```

### ❌ 陷阱 3：忘記 await

```csharp
// ❌ 錯誤：忘記 await
public async Task<Member> GetAsync(Guid id)
{
    var member = dbContext.Members.FindAsync(id);  // ❌ 忘記 await
    return member;  // ❌ 回傳 Task<Member> 而非 Member
}
```

## 參考資源

- 📚 [CLAUDE.md](../../../CLAUDE.md) - 完整專案指導文件
- 📝 [Repository Pattern](./repository-pattern.md) - Repository 設計策略
- 📝 [錯誤處理](./error-handling.md) - Result Pattern 使用方式
- 📝 [效能最佳化](./performance-optimization.md) - 查詢最佳化策略
