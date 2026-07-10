# Docker 優先測試策略

## 策略概述

本文檔描述採用 Docker 容器作為測試替身的核心原則、實踐與最佳實踐。這個策略確保測試環境與生產環境盡可能接近，提升測試的可信度。

## 核心原則

### 1. 真實環境而非 Mock

**主要目標**：使用真實的資料庫、快取、訊息佇列等服務進行測試，而非模擬物件。

**優勢**：
- ✅ 發現真實環境中才會出現的問題（例如連接池、鎖等）
- ✅ 避免 Mock 陷阱（測試通過但實際失敗）
- ✅ 無需維護複雜的 Mock 邏輯
- ✅ 提升團隊對測試結果的信心

**何時使用 Mock**：
- ⚠️ 僅限於無法使用 Docker 替身的外部服務（例如第三方 API）
- ✅ 優先順序：真實服務 > Docker 容器 > Mock

### 2. 容器隔離性

**每個測試獨立運行**：
- 測試使用獨立的 Docker 容器實例
- 測試之間無資料污染
- 支援並行執行多個測試
- 測試失敗不影響其他測試

**實踐方式**：
```csharp
// 每次測試建立新的容器
public class TestServer
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithPassword("YourStrong@Password")
        .Build();
    
    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        // 建立資料庫連線
    }
    
    public async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        // 容器自動清理
    }
}
```

### 3. 資料隔離與清理

**測試資料管理策略**：

#### 方案 A：每次測試獨立容器
```csharp
// 優點：完全隔離，無污染
// 缺點：啟動慢
[Fact]
public async Task Test1()
{
    var container = new MsSqlContainer(...);
    await container.StartAsync();
    try
    {
        // 執行測試
    }
    finally
    {
        await container.StopAsync();
    }
}
```

#### 方案 B：共享容器 + 交易回滾
```csharp
// 優點：執行快
// 缺點：需管理交易狀態
[Fact]
public async Task Test1()
{
    using var transaction = await dbContext.Database.BeginTransactionAsync();
    try
    {
        // 執行測試
    }
    finally
    {
        await transaction.RollbackAsync(); // 回滾所有變更
    }
}
```

#### 方案 C：種子資料 + 重設
```csharp
// 優點：平衡效能與清晰度
// 缺點：需定義基線資料
[Fact]
public async Task Test1()
{
    // 恢復至基線狀態
    await dbContext.ResetToSeedDataAsync();
    
    // 執行測試
    
    // 清理測試資料
    await dbContext.DeleteTestDataAsync();
}
```

**推薦方案**：混合使用
- 快速執行測試：使用共享容器 + 交易回滾
- 複雜情況：使用獨立容器確保完全隔離

## Docker 測試環境設置

### 常見容器組成

```yaml
# docker-compose.yml 示例
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: YourStrong@Password123
      ACCEPT_EULA: Y
    ports:
      - "1433:1433"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"
```

### Testcontainers 整合

```csharp
public class TestServer
{
    private readonly IContainer _sqlContainer;
    private readonly IContainer _redisContainer;

    public TestServer()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithPassword("YourStrong@Password")
            .Build();

        _redisContainer = new RedisBuilder()
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _sqlContainer.StartAsync(),
            _redisContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _sqlContainer.StopAsync(),
            _redisContainer.StopAsync()
        );
    }
}
```

## 測試替身優先順序

### 優先級排列

1. **真實服務**（最佳選擇）
   - 使用 Docker 容器運行的真實資料庫
   - 優點：完全逼真
   - 缺點：啟動速度

2. **Docker 容器**（強烈推薦）
   - Testcontainers 庫
   - 優點：真實環境 + 自動化管理
   - 缺點：需配置

3. **記憶體內替身**（特定情況）
   - 例如：SQLite 記憶體資料庫
   - 優點：快速啟動
   - 缺點：行為與生產不符

4. **Mock/Stub**（最後手段）
   - 外部 API、訊息服務
   - 優點：不依賴外部
   - 缺點：難以維護、容易失效

**決策流程**：
```
需要測試外部依賴？
    ├─ 能用 Docker 容器？ → 使用 Testcontainers
    ├─ 無法 Docker？ → 使用記憶體內替身
    └─ 都不行？ → 最後才考慮 Mock
```

## 效能優化

### 啟動優化

#### 1. 並行啟動容器
```csharp
// ✅ 正確：並行啟動
await Task.WhenAll(
    _sqlContainer.StartAsync(),
    _redisContainer.StartAsync()
);

// ❌ 錯誤：序列啟動
await _sqlContainer.StartAsync();
await _redisContainer.StartAsync();
```

#### 2. 共享容器生命週期
```csharp
// 所有測試共享一個容器實例
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<TestServer>
{
    // 同集合內的測試共享 TestServer
}
```

#### 3. 容器初始化快取
```csharp
public class OptimizedTestServer : IAsyncLifetime
{
    private readonly MsSqlContainer _container;
    
    public async Task InitializeAsync()
    {
        // 建立但不啟動
        await _container.StartAsync();
        
        // 預執行遷移
        await MigrateAsync();
    }
}
```

### 查詢優化

```csharp
// ✅ 使用投影，避免不必要的資料
var users = await dbContext.Users
    .AsNoTracking()
    .Select(u => new { u.Id, u.Email })
    .ToListAsync();

// ❌ 載入全部欄位
var users = await dbContext.Users
    .AsNoTracking()
    .ToListAsync();
```

## 常見陷阱與解決方案

### 陷阱 1：容器啟動逾時

**症狀**：測試經常超時

**解決方案**：
```csharp
// 增加等待時間
var container = new MsSqlBuilder()
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilCommandSucceeds("cat", "/var/opt/mssql/health_check.txt"))
    .WithStartupTimeout(TimeSpan.FromMinutes(2))
    .Build();
```

### 陷阱 2：磁碟空間不足

**症狀**：隨著測試次數增加，磁碟佔用快速增長

**解決方案**：
```csharp
// 定期清理停止的容器
docker container prune -f

// 設定容器自動清理
var container = new MsSqlBuilder()
    .WithAutoRemove(true)
    .Build();
```

### 陷阱 3：測試資料污染

**症狀**：測試偶發失敗，順序相依

**解決方案**：
```csharp
// 使用交易隔離
[Fact]
public async Task Test()
{
    using var transaction = await dbContext.Database
        .BeginTransactionAsync();
    try
    {
        // 執行測試
        await dbContext.SaveChangesAsync();
    }
    finally
    {
        await transaction.RollbackAsync();
    }
}
```

### 陷阱 4：連接池耗盡

**症狀**：「連接池已滿」或類似錯誤

**解決方案**：
```csharp
// 明確釋放連接
foreach (var connection in dbContext.Database.GetConnection() as SqlConnection)
{
    connection?.Close();
    connection?.Dispose();
}

// DbContextFactory 自動管理
await using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
{
    // 使用完畢自動關閉
}
```

### 陷阱 5：非確定性測試

**症狀**：同一測試偶發成功、失敗

**解決方案**：
```csharp
// ❌ 錯誤：依賴系統時間
if (DateTime.UtcNow > someDateTime) { }

// ✅ 正確：使用注入的時間提供者
public class Service(ITimeProvider timeProvider)
{
    public bool IsExpired(DateTime expiresAt) 
        => timeProvider.UtcNow > expiresAt;
}

// 測試中注入虛假時間
var mockTimeProvider = new MockTimeProvider { UtcNow = testTime };
```

## 並行測試配置

### xUnit 並行執行設置

```csharp
// xunit.runner.json
{
  "diagnosticMessages": false,
  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

### 測試隔離確保

```csharp
// 各測試使用不同的資料庫
[Collection("Database collection")]
public class OrderTests : IClassFixture<TestServer>
{
    private readonly TestServer _server;
    
    public OrderTests(TestServer server)
    {
        _server = server;
    }
    
    [Fact]
    public async Task CreateOrder_Should_Succeed()
    {
        // 測試 A
    }
}

[Collection("Cache collection")]
public class CacheTests : IClassFixture<TestServer>
{
    // 不同集合，可並行執行
}
```

## 總結

### 採用 Docker 優先策略的收益

| 面向 | 收益 |
|------|-----|
| **可信度** | 真實環境測試，更接近生產 |
| **速度** | 並行執行，自動化管理 |
| **維護性** | 無 Mock 邏輯，易於理解 |
| **除錯** | 容器狀態可檢查、持久化 |
| **一致性** | 本地、CI/CD 環境相同 |

### 最佳實踐核心

- ✅ **優先使用容器**，而非 Mock
- ✅ **隔離測試資料**，避免污染
- ✅ **並行執行**，提升效率
- ✅ **自動化清理**，節省資源
- ✅ **監控效能**，定期優化
