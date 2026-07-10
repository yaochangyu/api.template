# Docker Testcontainers 設定指南

## 概述

Testcontainers 是一個 Java/C# 庫，允許在測試中使用真實的 Docker 容器，而非 Mock。

**優勢**：
- ✅ 真實環境測試（SQL Server、Redis、Seq 等）
- ✅ 自動容器生命週期管理
- ✅ 容器隔離（每個測試獨立容器）
- ✅ 測試並行執行

**何時使用**：
- ✅ 整合測試（需要真實資料庫）
- ✅ BDD 端點測試（透過 WebApplicationFactory）
- ❌ 單元測試（應使用 Mock）

## 安裝

### NuGet 套件
```bash
dotnet add package Testcontainers.MsSql --version 3.10.0
dotnet add package Testcontainers.Redis --version 3.10.0
dotnet add package Testcontainers --version 3.10.0
```

### Docker 前置條件
- Docker 必須安裝且運行中
- Linux/Windows/macOS 都支援

## Docker Compose 配置

### 簡單配置（開發用）
```yaml
version: '3.8'

services:
  mssql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: "YourPassword123!"
    ports:
      - "1433:1433"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
```

### Testcontainers 配置（測試用）

在 C# 中，不使用 docker-compose.yml，而是透過 code：

```csharp
public class TestEnvironment : IAsyncLifetime
{
    private MsSqlContainer _mssqlContainer;
    private RedisContainer _redisContainer;
    
    public async Task InitializeAsync()
    {
        // 啟動 SQL Server 容器
        _mssqlContainer = new MsSqlBuilder()
            .WithPassword("YourPassword123!")
            .Build();
        
        await _mssqlContainer.StartAsync();
        
        // 啟動 Redis 容器
        _redisContainer = new RedisBuilder()
            .Build();
        
        await _redisContainer.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _mssqlContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
    
    public string MsSqlConnectionString => 
        _mssqlContainer.GetConnectionString();
    
    public int RedisPort => 
        _redisContainer.GetMappedPort(6379);
}
```

## TestServer 與 Docker 容器整合

### WebApplicationFactory 配置

```csharp
public class TestServerFactory : WebApplicationFactory<Program>
{
    private readonly MsSqlContainer _mssqlContainer;
    private readonly RedisContainer _redisContainer;
    
    public TestServerFactory()
    {
        _mssqlContainer = new MsSqlBuilder()
            .WithPassword("TestPassword123!")
            .Build();
        
        _redisContainer = new RedisBuilder().Build();
    }
    
    protected override async Task InitializeAsync()
    {
        // 1. 啟動容器
        await _mssqlContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        // 2. 等待容器就緒
        await Task.Delay(TimeSpan.FromSeconds(2));
    }
    
    protected override async Task DisposeAsync()
    {
        // 清理容器
        await _mssqlContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 移除現有的資料庫設定
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);
            
            // 使用測試容器的連線字串
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_msSqlContainer.GetConnectionString()));
            
            // 使用測試 Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = $"localhost:{_redisContainer.GetMappedPort(6379)}";
            });
        });
    }
    
    public string MsSqlConnectionString => 
        _mssqlContainer.GetConnectionString();
    
    public string RedisConnectionString => 
        $"localhost:{_redisContainer.GetMappedPort(6379)}";
}
```

## 容器生命週期管理

### 方案 A：測試 Fixture（推薦用於多個測試）
```csharp
public class TestContext : IAsyncLifetime
{
    private readonly MsSqlContainer _mssql;
    private readonly RedisContainer _redis;
    
    public TestContext()
    {
        _mssql = new MsSqlBuilder().WithPassword("Test123!").Build();
        _redis = new RedisBuilder().Build();
    }
    
    public async Task InitializeAsync()
    {
        await _mssql.StartAsync();
        await _redis.StartAsync();
        
        // 初始化資料庫架構
        await InitializeDatabaseAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _mssql.StopAsync();
        await _redis.StopAsync();
    }
    
    private async Task InitializeDatabaseAsync()
    {
        var connectionString = _mssql.GetConnectionString();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        // 建立表格、索引等
        var cmd = connection.CreateCommand();
        cmd.CommandText = "CREATE TABLE Members (...)";
        await cmd.ExecuteNonQueryAsync();
    }
}
```

### 方案 B：xUnit Collection Fixture
```csharp
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<TestContext>
{
}

[Collection("Database Collection")]
public class MemberTests
{
    private readonly TestContext _context;
    
    public MemberTests(TestContext context)
    {
        _context = context;
    }
    
    [Fact]
    public async Task CreateMember_Success()
    {
        // 使用 _context.MsSqlConnectionString
    }
}
```

## 常見陷阱

### ❌ 陷阱 1：每個測試建立新容器
```csharp
// 不要這樣做 — 太慢
[TestClass]
public class MemberTests
{
    [TestMethod]
    public async Task Test1()
    {
        var mssql = new MsSqlBuilder().Build();
        await mssql.StartAsync();
        // ... 測試
        await mssql.StopAsync();
    }
}
```

**改正**：使用 TestContext 或 Collection Fixture，多個測試共用一個容器。

### ❌ 陷阱 2：忘記等待容器就緒
```csharp
// 不要這樣做
await _mssql.StartAsync();
// 立即使用容器（容器可能還未就緒）
await dbContext.Database.MigrateAsync();
```

**改正**：新增延遲或健康檢查。
```csharp
await _mssql.StartAsync();
await Task.Delay(TimeSpan.FromSeconds(2));  // ✅ 等待就緒
await dbContext.Database.MigrateAsync();
```

### ❌ 陷阱 3：忘記清理容器
```csharp
// 不要這樣做 — 容器會繼續運行
public class Tests : IDisposable
{
    private MsSqlContainer _mssql;
    
    public void Dispose()
    {
        // ❌ 沒有停止容器
    }
}
```

**改正**：使用 IAsyncLifetime。
```csharp
public class Tests : IAsyncLifetime
{
    public async Task DisposeAsync()
    {
        await _mssql.StopAsync();  // ✅
    }
}
```

## 效能最佳化

### 1. 重用容器
```csharp
// ❌ 每個測試類建立新容器（12 個 test 類 = 12 個容器啟動）

// ✅ 使用共用 Collection Fixture（只啟動 1 次）
[Collection("Database Collection")]
public class Test1 { }

[Collection("Database Collection")]
public class Test2 { }
```

### 2. 並行測試
```csharp
// Testcontainers 自動隔離容器，支援並行執行
[Collection("Database Collection")]
public class ParallelTest1 { /* 獨立資料庫 */ }

[Collection("Other Collection")]
public class ParallelTest2 { /* 獨立資料庫 */ }
// 可並行執行，每個 Collection 一個容器
```

### 3. 快取映像
```csharp
// 首次啟動：下載映像（~30 秒）
// 後續啟動：使用快取（~3 秒）
// Docker 自動快取映像，無需手動配置
```

## 檢查清單

設定 Testcontainers 時確保：
- [ ] Docker 已安裝且運行中
- [ ] NuGet 套件已安裝（Testcontainers.MsSql 等）
- [ ] TestServer 使用容器連線字串
- [ ] 使用 IAsyncLifetime 管理容器生命週期
- [ ] 所有容器都在 DisposeAsync 中停止
- [ ] 多個測試類使用同一個 Collection Fixture
- [ ] 測試之間有資料隔離（新容器或清理資料）

---

**相關文檔**: `/bdd-testing` SKILL
