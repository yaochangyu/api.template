using System.Text.Json;
using FluentAssertions;
using JobBank1111.Infrastructure.Caching;
using JobBank1111.Testing.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Redis;
using Xunit;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

/// <summary>
/// CacheProvider 的 Redis 案例（health-check 2026-07-12 D7）。
/// 原在 JobBank1111.Job.Test，因需要真實 Redis 而遷入整合測試專案，
/// 依專案測試策略以 Testcontainers 起 Redis，本機／CI 無常駐 Redis 也能跑。
/// </summary>
public class CacheProviderRedisTest : IAsyncLifetime
{
    private RedisContainer _redisContainer = null!;
    private string? _originalRedisUrl;

    public async Task InitializeAsync()
    {
        _originalRedisUrl = Environment.GetEnvironmentVariable(nameof(SYS_REDIS_URL));
        _redisContainer = await TestContainerFactory.CreateRedisContainerAsync();
    }

    public async Task DisposeAsync()
    {
        // 還原環境變數，避免影響同一 test run 內其他測試（如 BDD 情境）讀到已停掉的容器
        Environment.SetEnvironmentVariable(nameof(SYS_REDIS_URL), _originalRedisUrl);
        await _redisContainer.DisposeAsync();
    }

    private ICacheProviderFactory CreateCacheProviderFactory()
    {
        Environment.SetEnvironmentVariable(nameof(SYS_REDIS_URL), _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable(nameof(DEFAULT_CACHE_EXPIRATION), "00:00:05");

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddEnvironmentVariables();
        var services = new ServiceCollection();
        var configuration = configurationBuilder.Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCacheProviderFactory(configuration);
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ICacheProviderFactory>();
    }

    [Fact]
    public async Task 寫讀快取資料_Json_Redis()
    {
        var cacheProviderFactory = CreateCacheProviderFactory();
        var cacheProvider = cacheProviderFactory.Create(CacheProviderType.Redis);

        var key = "Cache:Member:1";
        var expected = JsonSerializer.Serialize(new { Name = "小心肝" });
        await cacheProvider.SetAsync(key, expected);

        var result = await cacheProvider.GetAsync<string>(key);
        result.Should().Be(expected);
    }

    [Fact]
    public async Task 寫讀快取資料_String_Redis()
    {
        var cacheProviderFactory = CreateCacheProviderFactory();
        var cacheProvider = cacheProviderFactory.Create(CacheProviderType.Redis);

        var key = "Cache:Member:2";
        var expected = "小心肝";
        await cacheProvider.SetAsync(key, expected);

        var result = await cacheProvider.GetAsync<string>(key);
        result.Should().Be(expected);
    }
}
