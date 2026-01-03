using JobBank1111.Infrastructure.Caching;
using JobBank1111.Infrastructure.TraceContext;
using JobBank1111.Job.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace JobBank1111.Job.WebAPI;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEnvironments(this IServiceCollection services)
    {
        services.AddSingleton<SYS_DATABASE_CONNECTION_STRING>();
        return services;
    }

    public static IServiceCollection AddContextAccessor(this IServiceCollection services)
    {
        services.AddSingleton<ContextAccessor<TraceContext>>();
        services.AddSingleton<IContextGetter<TraceContext>>(p => p.GetService<ContextAccessor<TraceContext>>());
        services.AddSingleton<IContextSetter<TraceContext>>(p => p.GetService<ContextAccessor<TraceContext>>());
        return services;
    }

    public static void AddDatabase(this IServiceCollection services)
    {
        services.AddDbContextFactory<MemberDbContext>((provider,
                                                       builder) =>
        {
            var environment = provider.GetService<SYS_DATABASE_CONNECTION_STRING>();
            var connectionString = environment.Value;
            builder.UseSqlServer(connectionString)
                .UseLoggerFactory(provider.GetService<ILoggerFactory>())
                .EnableSensitiveDataLogging()
                ;
        });
    }

    public static IHttpClientBuilder AddExternalApiHttpClient(this IServiceCollection services)
    {
        return services.AddHttpClient("externalApi",
                                      (provider,
                                       client) =>
                                      {
                                          var traceContext = provider.GetService<TraceContext>();
                                          var externalApi = provider.GetService<EXTERNAL_API>();
                                          var traceId = traceContext.TraceId;
                                          client.BaseAddress = new Uri(externalApi.Value);
                                          client.DefaultRequestHeaders.Add(SysHeaderNames.TraceId, traceId);
                                      })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // 改成 true，會快取 Cookie
                UseCookies = false
            });
    }

    public static IServiceCollection AddSysEnvironments(this IServiceCollection services)
    {
        services.AddSingleton<SYS_DATABASE_CONNECTION_STRING>();
        services.AddSingleton<SYS_REDIS_URL>();
        return services;
    }

    public static IServiceCollection AddCacheProviderFactory(this IServiceCollection services,
                                                             IConfiguration configuration)
    {
        services.AddSingleton(p =>
        {
            var expiration = configuration.GetValue<TimeSpan>(nameof(DEFAULT_CACHE_EXPIRATION));
            var expirationToNow = DateTimeOffset.Now.Add(expiration);
            var absoluteExpiration = expirationToNow - DateTimeOffset.Now;
            var options = new CacheProviderOptions { AbsoluteExpiration = expiration };
            return options;
        });

        services.AddMemoryCache();
        services.AddStackExchangeRedisCache((options) =>
        {
            var connectionString = configuration.GetValue<string>(nameof(SYS_REDIS_URL));

            // options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
            // {
            //     EndPoints = { connectionString },
            //     DefaultDatabase = 0,
            // };

            options.Configuration = connectionString;

            // options.InstanceName = "SampleInstance";
        });

        services.AddSingleton<ICacheProviderFactory>(p =>
        {
            var memoryCache = p.GetService<IMemoryCache>();
            var distributedCache = p.GetService<IDistributedCache>();
            var cacheProviderOptions = p.GetService<CacheProviderOptions>();
            var factory = new CacheProviderFactory(memoryCache, distributedCache, cacheProviderOptions);
            return factory;
        });

        return services;
    }
}