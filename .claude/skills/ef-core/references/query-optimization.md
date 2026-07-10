# EF Core 查詢最佳化指南

本文詳細說明 EF Core 查詢最佳化的核心策略與實務技巧。

## 目錄
- [N+1 查詢問題](#n1-查詢問題)
- [Include/Join 策略](#includejoin-策略)
- [AsNoTracking 使用時機](#asnotracking-使用時機)
- [批次操作最佳實踐](#批次操作最佳實踐)
- [查詢性能度量](#查詢性能度量)
- [實際案例分析](#實際案例分析)

## N+1 查詢問題

### 常見陷阱：N+1 查詢

**問題定義**：執行 1 次主查詢後，每個結果都需要額外的子查詢，總共執行 N+1 次查詢。

#### ❌ 錯誤範例（觸發 N+1）
```csharp
// 執行 1 次查詢取得所有會員
var members = await dbContext.Members
    .AsNoTracking()
    .ToListAsync();

// 接著在迴圈中執行 N 次查詢（每個會員一次）
foreach (var member in members)
{
    var orders = dbContext.Orders
        .Where(o => o.MemberId == member.Id)
        .ToList(); // 第 2 到 N+1 次查詢！
    
    member.Orders = orders;
}
```

**執行 SQL 次數**：
- 第 1 次：SELECT * FROM Members (1 次)
- 第 2-N+1 次：SELECT * FROM Orders WHERE MemberId = ? (N 次，每個會員一次)
- **總計**：N+1 次查詢（100 個會員 = 101 次！）

### 性能影響

| 會員數 | 查詢次數 | 執行時間（估算） | 資料庫連線消耗 |
|------|--------|--------------|-------------|
| 10 | 11 次 | ~50ms | 中 |
| 100 | 101 次 | ~500ms | 高 |
| 1,000 | 1,001 次 | ~5s | 很高 |
| 10,000 | 10,001 次 | ~50s+ | 極高 |

### ✅ 解決方案：使用 Include

```csharp
// ✅ 正確：使用 Include 在一次查詢中載入相關資料
var members = await dbContext.Members
    .Include(m => m.Orders)
    .AsNoTracking()
    .ToListAsync();

// 執行 SQL：
// SELECT m.*, o.* FROM Members m
// LEFT JOIN Orders o ON m.Id = o.MemberId
```

**執行 SQL 次數**：1 次（即使有 1,000 個會員）

## Include/Join 策略

### 策略 1：Include（關聯式載入）

#### 優點
- ✅ 語法簡潔、易於閱讀
- ✅ 自動處理 JOIN 邏輯
- ✅ 支援複雜的巢狀關聯 (ThenInclude)

#### 缺點
- ❌ 可能產生笛卡爾積（重複資料）
- ❌ 載入不必要的欄位

```csharp
// 多層巢狀
var members = await dbContext.Members
    .Include(m => m.Orders)
        .ThenInclude(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
    .AsNoTracking()
    .ToListAsync();

// 執行 SQL：複雜的多表 JOIN
```

### 策略 2：Join + Select（投影）

#### 優點
- ✅ 精確控制載入的欄位
- ✅ 避免笛卡爾積
- ✅ 效能最優

#### 缺點
- ❌ 語法較複雜
- ❌ 需要手動 DTO 對映

```csharp
// 只載入需要的欄位
var members = await dbContext.Members
    .Join(
        dbContext.Orders,
        m => m.Id,
        o => o.MemberId,
        (m, o) => new
        {
            MemberId = m.Id,
            MemberName = m.Name,
            OrderId = o.Id,
            OrderDate = o.OrderDate
        }
    )
    .AsNoTracking()
    .ToListAsync();
```

### 選擇指南

| 情況 | 推薦策略 | 理由 |
|-----|--------|------|
| 1-2 層關聯，需要完整物件 | Include | 語法簡潔 |
| 3 層以上深度關聯 | Select 投影 | 避免笛卡爾積 |
| 只需部分欄位 | Select 投影 | 減少資料轉移 |
| 複雜業務邏輯 | 原始 SQL + FromSqlInterpolated | 最大控制 |

## AsNoTracking 使用時機

### 使用情景

#### ✅ 使用 AsNoTracking（讀取）
- 查詢頁面展示資料
- API 回傳唯讀結果
- 報表查詢
- 分析資料蒐集

#### ❌ 不使用 AsNoTracking（寫入）
- 查詢後需要修改實體
- 需要樂觀併發控制 (RowVersion)
- 需要 ChangeTracker 追蹤

### 效能對比

```csharp
// ❌ 不加 AsNoTracking：有變更追蹤
var orders = await dbContext.Orders
    .Where(o => o.Status == "Pending")
    .ToListAsync();
// 記憶體消耗：100MB（追蹤 1,000 筆）
// CPU 消耗：高（快照對比）

// ✅ 加 AsNoTracking：無變更追蹤
var orders = await dbContext.Orders
    .Where(o => o.Status == "Pending")
    .AsNoTracking()
    .ToListAsync();
// 記憶體消耗：30MB（無追蹤）
// CPU 消耗：低
```

### 效能數據

- **AsNoTracking 查詢速度**：快 15-40%（取決於資料量）
- **記憶體節省**：30-60%（取決於實體大小）

## 批次操作最佳實踐

### 問題：迴圈插入造成的性能問題

#### ❌ 錯誤：逐筆插入
```csharp
var products = new List<Product>();
for (int i = 0; i < 10000; i++)
{
    var product = new Product { Name = $"Product{i}" };
    dbContext.Products.Add(product);
    await dbContext.SaveChangesAsync(); // 10,000 次提交！
}
```

**性能**：~30 秒（10,000 次資料庫往返）

### ✅ 解決方案：批次插入

```csharp
// 方案 1：AddRange + 單次 SaveChanges
var products = Enumerable.Range(0, 10000)
    .Select(i => new Product { Name = $"Product{i}" })
    .ToList();

dbContext.Products.AddRange(products);
await dbContext.SaveChangesAsync(); // 1 次提交
```

**性能**：~1 秒（減少 97%）

### BulkInsert 與 BulkUpdate

使用 EF Core 擴展套件（如 EFCore.BulkExtensions）以獲得最佳效能：

```csharp
// 需要 NuGet：EFCore.BulkExtensions
using EFCore.BulkExtensions;

var products = Enumerable.Range(0, 100000)
    .Select(i => new Product { Name = $"Product{i}" })
    .ToList();

// 批次插入
await dbContext.BulkInsertAsync(products);

// 批次更新
await dbContext.BulkUpdateAsync(products);

// 批次刪除
await dbContext.BulkDeleteAsync(products);
```

**性能對比**：
- AddRange + SaveChanges：~10 秒（100,000 筆）
- BulkInsert：~2 秒（快 5 倍）

## 查詢性能度量

### 方案 1：EF Core 查詢記錄

```csharp
// 在 Program.cs 配置
services.AddDbContextFactory<JobBankDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    
    // 啟用查詢記錄
    options.LogTo(Console.WriteLine, LogLevel.Information);
});
```

### 方案 2：執行時間測量

```csharp
public class QueryPerformanceMonitor
{
    public async Task<QueryResult<T>> MeasureAsync<T>(
        Func<Task<T>> query,
        string operationName)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var result = await query();
            watch.Stop();
            
            return new QueryResult<T>
            {
                Data = result,
                ExecutionTimeMs = watch.ElapsedMilliseconds,
                OperationName = operationName,
                Success = true
            };
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new QueryResult<T>
            {
                ExecutionTimeMs = watch.ElapsedMilliseconds,
                OperationName = operationName,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
```

## 實際案例分析

### 案例 1：訂單列表查詢優化

#### ❌ 原始版本（慢）
```csharp
var orders = await dbContext.Orders
    .Where(o => o.CreatedAt > DateTime.Now.AddDays(-30))
    .ToListAsync();

// 接著在 Controller 中迴圈存取
foreach (var order in orders)
{
    order.MemberName = dbContext.Members
        .Where(m => m.Id == order.MemberId)
        .Select(m => m.Name)
        .FirstOrDefault();
    
    order.ItemCount = dbContext.OrderItems
        .Where(oi => oi.OrderId == order.Id)
        .Count(); // N+1 查詢！
}
```

#### ✅ 優化版本（快）
```csharp
var orders = await dbContext.Orders
    .Where(o => o.CreatedAt > DateTime.Now.AddDays(-30))
    .Include(o => o.Member)
    .Include(o => o.OrderItems)
    .AsNoTracking()
    .Select(o => new OrderSummary
    {
        Id = o.Id,
        OrderDate = o.OrderDate,
        MemberName = o.Member.Name,
        ItemCount = o.OrderItems.Count,
        Total = o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
    })
    .ToListAsync();
```

**效能提升**：從 1,000 次查詢 → 1 次查詢，執行時間從 5 秒 → 200ms

### 案例 2：權限檢查查詢優化

#### ✅ 優化的權限檢查
```csharp
public async Task<bool> HasPermissionAsync(
    Guid userId, 
    string permission,
    CancellationToken ct = default)
{
    // ✅ 直接檢查，避免載入整個使用者物件
    return await dbContext.UserPermissions
        .AsNoTracking()
        .Where(up => up.UserId == userId)
        .Where(up => up.Permission == permission)
        .AnyAsync(ct); // AnyAsync 一旦找到就返回，效能最佳
}
```

## 最佳實踐檢查清單

- [ ] 避免在迴圈中執行查詢（N+1 問題）
- [ ] 讀取查詢使用 AsNoTracking()
- [ ] 複雜查詢使用 Include 或 Select 投影
- [ ] 大量資料使用分頁（Skip/Take）
- [ ] 批次操作使用 AddRange + SaveChanges 或 BulkExtensions
- [ ] 關鍵查詢進行性能測試
- [ ] 使用 EF Core 查詢記錄偵測 N+1 問題
