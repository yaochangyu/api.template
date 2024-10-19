using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace JobBank1111.Testing.Common;

public class TestContainerFactory
{
    public static async Task<RedisContainer> CreateRedisContainerAsync()
    {
        var redisContainer = new RedisBuilder()
            .WithImage("redis:7.0")
            .Build();
        await redisContainer.StartAsync();
        return redisContainer;
    }

    public static async Task<MsSqlContainer> CreateMsSqlContainerAsync()
    {
        var mssqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithPassword("Password123")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_PID", "Developer")
            .WithPortBinding(1433, assignRandomHostPort: true)
            .Build();
        await mssqlContainer.StartAsync();
        return mssqlContainer;
    }

    public static async Task<PostgreSqlContainer> CreatePostgreSqlContainerAsync()
    {
        var waitStrategy = Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready");
        var postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:13-alpine")
            .WithName("postgres.13")
            .WithPortBinding(5432, assignRandomHostPort: true)
            .WithWaitStrategy(waitStrategy)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await postgreSqlContainer.StartAsync();
        return postgreSqlContainer;
    }
}