# Repository Pattern 最佳實踐

## 📋 目錄
- [什麼是 Repository Pattern](#什麼是-repository-pattern)
- [核心概念](#核心概念)
- [設計原則](#設計原則)
- [實作策略](#實作策略)
- [最佳實踐](#最佳實踐)
- [常見反模式](#常見反模式)
- [實作範例](#實作範例)

---

## 什麼是 Repository Pattern

**定義 (Martin Fowler):**
> Repository 在領域層和資料對應層之間進行仲介，使用類似集合的介面來存取領域物件。

**目的:**
- 將領域模型與資料存取細節隔離
- 提供物件導向的資料持久層視圖
- 集中查詢邏輯,減少重複程式碼
- 達成領域層與資料對應層之間的清晰分離和單向依賴

---

## 核心概念

### 1. Repository 的職責

```
┌─────────────────┐
│  Domain Layer   │ ← 定義 IRepository 介面
└────────┬────────┘
         │ 依賴
┌────────▼────────┐
│ Infrastructure  │ ← 實作 Repository 類別
│     Layer       │
└────────┬────────┘
         │ 存取
┌────────▼────────┐
│    Database     │
└─────────────────┘
```

**Repository 應該:**
- ✅ 封裝資料存取邏輯
- ✅ 提供類似集合的操作介面
- ✅ 將資料查詢與業務邏輯分離
- ✅ 支援單元測試 (透過抽象介面)

**Repository 不應該:**
- ❌ 包含業務邏輯
- ❌ 直接暴露 ORM 特定功能
- ❌ 違反單一職責原則
- ❌ 繞過 Aggregate Root 的邊界

---

## 設計原則

### 原則 1: 每個 Aggregate Root 一個 Repository

**DDD 核心規則:**
- 只為 Aggregate Root 建立 Repository
- Repository 與 Aggregate Root 一對一關係
- 透過 Aggregate Root 維護事務一致性

```csharp
// ✅ 正確: Aggregate Root 有 Repository
public interface IOrderRepository : IRepository<Order>
{
    Order Add(Order order);
    Order GetById(OrderId id);
    void Update(Order order);
}

// ❌ 錯誤: 不為 Entity 建立獨立 Repository
// public interface IOrderItemRepository { } // OrderItem 是 Order 的一部分
```

### 原則 2: Repository 介面定義在 Domain Layer

**分層依賴規則:**
```
Domain Layer (介面定義)
    ↑
    │ 實作
    │
Infrastructure Layer (具體實作)
```

```csharp
// Domain Layer
namespace YourApp.Domain.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order> GetByIdAsync(OrderId id, CancellationToken ct);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken ct);
    }
}

// Infrastructure Layer
namespace YourApp.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        
        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }
        
        // 實作細節...
    }
}
```

### 原則 3: 使用 Generic Constraints 強制 Aggregate Root

```csharp
// 標記介面
public interface IAggregateRoot { }

// 通用 Repository 基底介面
public interface IRepository<T> where T : class, IAggregateRoot
{
    Task<T> GetByIdAsync(int id, CancellationToken ct);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}

// Aggregate Root 實作
public class Order : IAggregateRoot
{
    public OrderId Id { get; private set; }
    // ...
}
```

### 原則 4: CQRS 分離查詢與命令

**查詢 (Query):**
- 不改變資料狀態
- 可以繞過 Repository,直接查詢資料庫
- 使用 Dapper 等輕量級工具提升效能

**命令 (Command):**
- 改變資料狀態
- 必須透過 Repository 和 Aggregate Root
- 維護事務一致性

```csharp
// ✅ 查詢: 可以使用 Dapper 直接查詢
public class GetOrdersQueryHandler
{
    private readonly IDbConnection _connection;
    
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery query)
    {
        var sql = "SELECT * FROM Orders WHERE CustomerId = @CustomerId";
        return await _connection.QueryAsync<OrderDto>(sql, new { query.CustomerId });
    }
}

// ✅ 命令: 必須透過 Repository
public class CreateOrderCommandHandler
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<Order>> Handle(CreateOrderCommand command)
    {
        var order = Order.Create(command.CustomerId, command.Items);
        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success(order);
    }
}
```

---

## 實作策略

### 策略 1: 資料表導向 Repository (Table-Oriented)

**適用情境:**
- 簡單 CRUD 應用
- 資料結構與資料庫表格高度對應
- 領域邏輯較少

```csharp
public interface IProductRepository
{
    Task<Product> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);
}
```

**優點:**
- 實作簡單直觀
- 與資料庫結構對應清楚

**缺點:**
- 容易造成貧血模型 (Anemic Domain Model)
- 領域邏輯容易散落各處

### 策略 2: 需求導向 Repository (Use Case-Oriented)

**適用情境:**
- 複雜業務邏輯
- DDD 設計
- 特定業務場景

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<Order> GetByIdWithItemsAsync(OrderId id, CancellationToken ct);
    Task<IEnumerable<Order>> GetPendingOrdersByCustomerAsync(CustomerId customerId, CancellationToken ct);
    Task<IEnumerable<Order>> GetOrdersReadyToShipAsync(CancellationToken ct);
    Task<bool> HasActiveOrdersAsync(CustomerId customerId, CancellationToken ct);
}
```

**優點:**
- 明確表達業務意圖
- 支援複雜查詢邏輯
- 提升可讀性與維護性

**缺點:**
- 需要更多方法定義
- 可能產生過多特定方法

### 策略 3: 混合模式 (Hybrid)

**結合基本 CRUD + 特定業務方法:**

```csharp
public interface IOrderRepository : IRepository<Order>
{
    // 基本操作 (繼承自 IRepository<T>)
    // - GetByIdAsync
    // - Add
    // - Update
    // - Remove
    
    // 業務特定方法
    Task<Order> GetOrderWithDetailsAsync(OrderId id, CancellationToken ct);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken ct);
    Task<decimal> GetTotalRevenueByCustomerAsync(CustomerId customerId, CancellationToken ct);
}
```

---

## 最佳實踐

### ✅ 實踐 1: EF Core 已實作 Repository + Unit of Work

**重要:** DbContext 本身已經是 Repository 和 Unit of Work 的實作。

```csharp
// ❌ 避免: 建立通用 Generic Repository 包裝 DbContext
public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    
    public IQueryable<T> GetAll() => _context.Set<T>();
    // ... 只是簡單包裝 DbContext
}

// ✅ 推薦: 直接使用 DbContext 或建立特定領域 Repository
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    
    public async Task<Order> GetByIdAsync(OrderId id, CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }
    
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct)
    {
        return await _context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(ct);
    }
}
```

**原因:**
- EF Core 的 `DbContext` 已經提供 Change Tracking
- `DbContext.Set<T>()` 已經是 Repository
- `SaveChangesAsync()` 已經是 Unit of Work
- 額外的抽象層只會增加複雜度而無價值

### ✅ 實踐 2: Repository 方法應該有明確業務意圖

```csharp
// ❌ 避免: 通用查詢方法
Task<IEnumerable<Order>> FindWhere(Expression<Func<Order, bool>> predicate);

// ✅ 推薦: 明確業務意圖
Task<IEnumerable<Order>> GetActiveOrdersByCustomerAsync(CustomerId customerId, CancellationToken ct);
Task<IEnumerable<Order>> GetOrdersAwaitingPaymentAsync(CancellationToken ct);
```

### ✅ 實踐 3: 使用非同步方法

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<Order> GetByIdAsync(OrderId id, CancellationToken ct);
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    // 注意: Add/Remove 通常是同步的,因為只是標記狀態
}
```

### ✅ 實踐 4: 支援測試性 - 使用介面抽象

```csharp
// 測試時使用 Mock Repository
public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_ShouldSucceed()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var service = new OrderService(mockRepo.Object);
        
        // Act
        var result = await service.CreateOrderAsync(customerId, items);
        
        // Assert
        Assert.True(result.IsSuccess);
        mockRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
    }
}
```

### ✅ 實踐 5: 使用規格模式 (Specification Pattern) 處理複雜查詢

```csharp
// 規格介面
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}

// 具體規格
public class PendingOrdersSpecification : ISpecification<Order>
{
    public Expression<Func<Order, bool>> ToExpression()
    {
        return order => order.Status == OrderStatus.Pending;
    }
    
    public bool IsSatisfiedBy(Order entity)
    {
        return entity.Status == OrderStatus.Pending;
    }
}

// Repository 使用規格
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetBySpecificationAsync(ISpecification<Order> spec, CancellationToken ct);
}

public class OrderRepository : IOrderRepository
{
    public async Task<IEnumerable<Order>> GetBySpecificationAsync(
        ISpecification<Order> spec, 
        CancellationToken ct)
    {
        return await _context.Orders
            .Where(spec.ToExpression())
            .ToListAsync(ct);
    }
}
```

### ✅ 實踐 6: 使用 DbContextFactory 在長時間運作場景

```csharp
// 適用於 Background Services, SignalR Hubs
public class OrderRepository : IOrderRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    
    public OrderRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task<Order> GetByIdAsync(OrderId id, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }
}
```

---

## 常見反模式

### ❌ 反模式 1: 通用 Generic Repository 無業務價值

**問題:**
```csharp
public class GenericRepository<T> : IRepository<T> where T : class
{
    public IQueryable<T> GetAll() => _context.Set<T>();
    public T GetById(int id) => _context.Set<T>().Find(id);
    public void Add(T entity) => _context.Set<T>().Add(entity);
    // ... 只是包裝 DbContext
}
```

**為什麼不好:**
- 只是 DbContext 的簡單包裝,沒有提供額外價值
- 無法表達業務意圖
- 增加不必要的抽象層
- EF Core 已經提供這些功能

**Microsoft 官方建議:**
> DbContext 本身已經實作了 Repository 和 Unit of Work 模式,不需要額外包裝。

### ❌ 反模式 2: 暴露 IQueryable

**問題:**
```csharp
public interface IOrderRepository
{
    IQueryable<Order> GetAll(); // ❌ 暴露 IQueryable
}

// 使用端
var orders = orderRepository.GetAll()
    .Where(o => o.Status == OrderStatus.Pending)
    .OrderBy(o => o.CreatedAt)
    .ToList(); // 查詢邏輯散落在各處
```

**為什麼不好:**
- 洩漏資料存取實作細節
- 查詢邏輯散落在應用層
- 難以測試
- 無法控制查詢效能

**改善:**
```csharp
public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken ct);
}
```

### ❌ 反模式 3: Repository 包含業務邏輯

**問題:**
```csharp
public class OrderRepository : IOrderRepository
{
    public async Task<Result> CreateOrderAsync(CreateOrderDto dto)
    {
        // ❌ 業務驗證邏輯不應該在 Repository
        if (dto.Items.Count == 0)
            return Result.Failure("訂單必須包含商品");
            
        var order = new Order { ... };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}
```

**改善:**
```csharp
// Domain Service / Handler 處理業務邏輯
public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    
    public async Task<Result<Order>> Handle(CreateOrderCommand cmd)
    {
        // ✅ 業務驗證在這裡
        var orderResult = Order.Create(cmd.CustomerId, cmd.Items);
        if (orderResult.IsFailure)
            return Result.Failure<Order>(orderResult.Error);
            
        // Repository 只負責資料存取
        _repository.Add(orderResult.Value);
        await _unitOfWork.SaveChangesAsync();
        
        return Result.Success(orderResult.Value);
    }
}
```

### ❌ 反模式 4: 為每個資料表建立 Repository

**問題:**
```csharp
// ❌ 不是 Aggregate Root 也建立 Repository
public interface IOrderItemRepository { }
public interface IAddressRepository { }
public interface IPaymentRepository { }
```

**改善:**
```csharp
// ✅ 只為 Aggregate Root 建立 Repository
public interface IOrderRepository : IRepository<Order>
{
    // Order 是 Aggregate Root
    // OrderItem, Address, Payment 透過 Order 存取
}

// 透過 Aggregate Root 操作
var order = await _orderRepository.GetByIdAsync(orderId);
order.AddItem(product, quantity); // ✅ 透過 Order 操作 OrderItem
order.UpdateShippingAddress(newAddress); // ✅ 透過 Order 操作 Address
await _unitOfWork.SaveChangesAsync();
```

---

## 實作範例

### 完整範例: 訂單管理系統

#### 1. Domain Layer - 介面定義

```csharp
// Domain/Common/IAggregateRoot.cs
namespace YourApp.Domain.Common
{
    public interface IAggregateRoot { }
}

// Domain/Common/IRepository.cs
namespace YourApp.Domain.Common
{
    public interface IRepository<T> where T : class, IAggregateRoot
    {
        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}

// Domain/Orders/Order.cs
namespace YourApp.Domain.Orders
{
    public class Order : IAggregateRoot
    {
        public OrderId Id { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        
        private Order() { } // EF Core
        
        public static Result<Order> Create(CustomerId customerId, IEnumerable<OrderItem> items)
        {
            if (items == null || !items.Any())
                return Result.Failure<Order>("訂單必須包含至少一項商品");
                
            var order = new Order
            {
                Id = OrderId.CreateNew(),
                CustomerId = customerId,
                Status = OrderStatus.Pending
            };
            
            order._items.AddRange(items);
            return Result.Success(order);
        }
        
        public Result ConfirmOrder()
        {
            if (Status != OrderStatus.Pending)
                return Result.Failure("只能確認待處理的訂單");
                
            Status = OrderStatus.Confirmed;
            return Result.Success();
        }
    }
}

// Domain/Orders/IOrderRepository.cs
namespace YourApp.Domain.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct);
        Task<Order?> GetByIdWithItemsAsync(OrderId id, CancellationToken ct);
        Task<IEnumerable<Order>> GetPendingOrdersByCustomerAsync(CustomerId customerId, CancellationToken ct);
        Task<bool> ExistsAsync(OrderId id, CancellationToken ct);
    }
}
```

#### 2. Infrastructure Layer - 具體實作

```csharp
// Infrastructure/Data/AppDbContext.cs
namespace YourApp.Infrastructure.Data
{
    public class AppDbContext : DbContext, IUnitOfWork
    {
        public DbSet<Order> Orders => Set<Order>();
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
        
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await base.SaveChangesAsync(ct);
        }
    }
}

// Infrastructure/Repositories/OrderRepository.cs
namespace YourApp.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        
        public OrderRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public void Add(Order entity)
        {
            _context.Orders.Add(entity);
        }
        
        public void Update(Order entity)
        {
            _context.Orders.Update(entity);
        }
        
        public void Remove(Order entity)
        {
            _context.Orders.Remove(entity);
        }
        
        public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }
        
        public async Task<Order?> GetByIdWithItemsAsync(OrderId id, CancellationToken ct)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .AsSplitQuery() // 避免笛卡爾爆炸
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }
        
        public async Task<IEnumerable<Order>> GetPendingOrdersByCustomerAsync(
            CustomerId customerId, 
            CancellationToken ct)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId && o.Status == OrderStatus.Pending)
                .ToListAsync(ct);
        }
        
        public async Task<bool> ExistsAsync(OrderId id, CancellationToken ct)
        {
            return await _context.Orders
                .AnyAsync(o => o.Id == id, ct);
        }
    }
}
```

#### 3. Application Layer - 使用 Repository

```csharp
// Application/Orders/Commands/CreateOrderCommand.cs
namespace YourApp.Application.Orders.Commands
{
    public record CreateOrderCommand(CustomerId CustomerId, List<OrderItemDto> Items);
    
    public class CreateOrderCommandHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result<OrderId>> Handle(CreateOrderCommand command, CancellationToken ct)
        {
            // 1. 建立領域物件
            var items = command.Items.Select(dto => 
                new OrderItem(dto.ProductId, dto.Quantity, dto.Price));
            
            var orderResult = Order.Create(command.CustomerId, items);
            if (orderResult.IsFailure)
                return Result.Failure<OrderId>(orderResult.Error);
            
            // 2. 透過 Repository 儲存
            var order = orderResult.Value;
            _orderRepository.Add(order);
            
            // 3. 透過 Unit of Work 提交
            await _unitOfWork.SaveChangesAsync(ct);
            
            return Result.Success(order.Id);
        }
    }
}

// Application/Orders/Queries/GetOrderQuery.cs (CQRS Query)
namespace YourApp.Application.Orders.Queries
{
    public record GetOrderQuery(OrderId OrderId);
    
    public class GetOrderQueryHandler
    {
        private readonly IDbConnection _connection; // 使用 Dapper 查詢
        
        public GetOrderQueryHandler(IDbConnection connection)
        {
            _connection = connection;
        }
        
        public async Task<OrderDto?> Handle(GetOrderQuery query, CancellationToken ct)
        {
            const string sql = @"
                SELECT o.Id, o.CustomerId, o.Status, o.CreatedAt,
                       i.ProductId, i.Quantity, i.Price
                FROM Orders o
                LEFT JOIN OrderItems i ON o.Id = i.OrderId
                WHERE o.Id = @OrderId";
            
            var orderDict = new Dictionary<Guid, OrderDto>();
            
            await _connection.QueryAsync<OrderDto, OrderItemDto, OrderDto>(
                sql,
                (order, item) =>
                {
                    if (!orderDict.TryGetValue(order.Id, out var orderEntry))
                    {
                        orderEntry = order;
                        orderEntry.Items = new List<OrderItemDto>();
                        orderDict.Add(order.Id, orderEntry);
                    }
                    
                    if (item != null)
                        orderEntry.Items.Add(item);
                    
                    return orderEntry;
                },
                new { OrderId = query.OrderId.Value },
                splitOn: "ProductId");
            
            return orderDict.Values.FirstOrDefault();
        }
    }
}
```

#### 4. Dependency Injection 註冊

```csharp
// Program.cs 或 Startup.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        
        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // Dapper 連線 (用於查詢)
        services.AddScoped<IDbConnection>(sp =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));
        
        return services;
    }
}
```

#### 5. 單元測試範例

```csharp
// Tests/Application/Orders/CreateOrderCommandHandlerTests.cs
public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        
        var handler = new CreateOrderCommandHandler(mockRepo.Object, mockUoW.Object);
        
        var command = new CreateOrderCommand(
            CustomerId.From(Guid.NewGuid()),
            new List<OrderItemDto>
            {
                new(ProductId.From(1), 2, 100m)
            });
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        mockRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        mockUoW.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_EmptyItems_ShouldReturnFailure()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var handler = new CreateOrderCommandHandler(mockRepo.Object, mockUoW.Object);
        
        var command = new CreateOrderCommand(
            CustomerId.From(Guid.NewGuid()),
            new List<OrderItemDto>()); // 空的項目
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("訂單必須包含至少一項商品", result.Error);
        mockRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }
}
```

---

## 總結

### Repository Pattern 的價值

✅ **應該使用 Repository 的情境:**
- DDD 複雜領域模型
- 需要強型別業務查詢
- 支援單元測試 (透過 Mock)
- 將領域邏輯與資料存取分離

❌ **不需要 Repository 的情境:**
- 簡單 CRUD 應用
- 直接使用 EF Core DbContext 已足夠
- 避免過度工程化

### 核心要點

1. **每個 Aggregate Root 一個 Repository** - 維護事務邊界
2. **介面定義在 Domain Layer** - 依賴反轉原則
3. **EF Core DbContext 已經是 Repository + UoW** - 避免不必要的抽象
4. **CQRS 分離查詢與命令** - 查詢可繞過 Repository
5. **方法應表達業務意圖** - 提升可讀性
6. **支援測試性** - 使用介面抽象

### 參考資料

- [Martin Fowler - Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Microsoft - Repository Pattern in DDD](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Eric Evans - Domain-Driven Design](https://www.domainlanguage.com/ddd/)

---

**最後更新:** 2026-01-05  
**作者:** GitHub Copilot + Context7 整合文件
