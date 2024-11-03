using System.Text.Json;
using JobBank1111.Infrastructure.Caching;
using JobBank1111.Job.WebAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobBank1111.Job.Test;

public class CacheProviderFactoryTest
{
    private static ICacheProviderFactory? CreateCacheProviderFactory()
    {
        Environment.SetEnvironmentVariable(nameof(SYS_REDIS_URL), "localhost:6379");
        Environment.SetEnvironmentVariable(nameof(DEFAULT_CACHE_EXPIRATION), "00:00:05");

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddEnvironmentVariables()
            ;
        var services = new ServiceCollection();
        var configuration = configurationBuilder.Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCacheProviderFactory(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var cacheProviderFactory = serviceProvider.GetService<ICacheProviderFactory>();
        return cacheProviderFactory;
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
        Assert.Equal(expected, result);
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
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task 寫讀快取資料_Json_Memory()
    {
        var cacheProviderFactory = CreateCacheProviderFactory();
        var cacheProvider = cacheProviderFactory.Create(CacheProviderType.Memory);

        var key = "Cache:Member:1";
        var expected = JsonSerializer.Serialize(new { Name = "小心肝" });
        await cacheProvider.SetAsync(key, expected);

        var result = await cacheProvider.GetAsync<string>(key);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task 寫讀快取資料_String_Memory()
    {
        var cacheProviderFactory = CreateCacheProviderFactory();
        var cacheProvider = cacheProviderFactory.Create(CacheProviderType.Memory);

        var key = "Cache:Member:2";
        var expected = "小心肝";
        await cacheProvider.SetAsync(key, expected);

        var result = await cacheProvider.GetAsync<string>(key);
        Assert.Equal(expected, result);
    }
}