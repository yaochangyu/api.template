using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace JobBank1111.Job.IntegrationTest;

/// <summary>
/// 測試伺服器工廠 — 自動管理 Docker 容器的完整生命週期
/// </summary>
public class TestServerFactory : WebApplicationFactory<Program>
{
    private MsSqlContainer _mssqlContainer;
    private RedisContainer _redisContainer;

    public string MsSqlConnectionString { get; private set; }
    public string RedisConnectionString { get; private set; }

    protected override async Task InitializeAsync()
    {
        // 1. 建立容器
        _mssqlContainer = new MsSqlBuilder()
            .WithPassword("TestPassword123!")
            .Build();

        _redisContainer = new RedisBuilder()
            .Build();

        // 2. 啟動容器
        await _mssqlContainer.StartAsync();
        await _redisContainer.StartAsync();

        // 3. 等待容器就緒
        await Task.Delay(TimeSpan.FromSeconds(2));

        // 4. 保存連線字串
        MsSqlConnectionString = _mssqlContainer.GetConnectionString();
        RedisConnectionString = $"localhost:{_redisContainer.GetMappedPort(6379)}";
    }

    protected override async Task DisposeAsync()
    {
        if (_mssqlContainer != null)
            await _mssqlContainer.StopAsync();

        if (_redisContainer != null)
            await _redisContainer.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 移除現有的 DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // 使用測試資料庫
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(MsSqlConnectionString));

            // 使用測試 Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = RedisConnectionString;
            });
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // 覆蓋測試專用配置
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Logging:LogLevel:Default"] = "Debug"
            });
        });
    }
}

/// <summary>
/// 測試 Fixture — 用於 xUnit Collection Fixture
/// </summary>
public class TestContext : IAsyncLifetime
{
    private readonly TestServerFactory _factory;
    public HttpClient HttpClient { get; private set; }

    public async Task InitializeAsync()
    {
        _factory = new TestServerFactory();
        HttpClient = _factory.CreateClient();

        // 初始化資料庫
        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        await _factory.DisposeAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        // 執行資料庫遷移
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}

/// <summary>
/// xUnit Collection 定義
/// </summary>
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<TestContext>
{
}

/// <summary>
/// 使用範例
/// </summary>
[Collection("Database Collection")]
public class MemberIntegrationTests
{
    private readonly TestContext _context;

    public MemberIntegrationTests(TestContext context)
    {
        _context = context;
    }

    [Fact]
    public async Task CreateMember_WithValidEmail_ReturnsCreated()
    {
        // Arrange
        var request = new { Email = "test@example.com", Name = "Test User" };

        // Act
        var response = await _context.HttpClient.PostAsJsonAsync(
            "/api/v1/members",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsAsync<dynamic>();
        Assert.NotNull(content.id);
    }

    [Fact]
    public async Task CreateMember_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new { Email = "duplicate@example.com", Name = "User 1" };

        await _context.HttpClient.PostAsJsonAsync("/api/v1/members", request);

        // Act
        var response = await _context.HttpClient.PostAsJsonAsync(
            "/api/v1/members",
            new { Email = "duplicate@example.com", Name = "User 2" });

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
