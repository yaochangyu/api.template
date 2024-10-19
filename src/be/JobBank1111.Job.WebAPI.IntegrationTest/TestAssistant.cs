
using JobBank1111.Job.WebAPI.IntegrationTest.MockServers;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

class TestAssistant
{
    public static void SetEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("N1_MARKET", "TW");
        Environment.SetEnvironmentVariable("N1_ENVIRONMENT", "QA");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("N1_LOCAL_MODE", "true");
        Environment.SetEnvironmentVariable("AUTHORIZED_USERS", "[]");
        Environment.SetEnvironmentVariable("IMS_REDIS_DOMAIN", "ims-cache-write.qa.91dev.tw:6379");
        Environment.SetEnvironmentVariable("IMS_DATABASE_HOST", "localhost");
        Environment.SetEnvironmentVariable("IMS_DATABASE_USER", "postgres");
        Environment.SetEnvironmentVariable("IMS_DATABASE_PASSWORD", "guest");
        Environment.SetEnvironmentVariable("IMS_DATABASE_NAME", "ims_test");
        Environment.SetEnvironmentVariable("IMS_AWS_S3_ACCESS_KEY", "AKIAIOSFODNN7EXAMPLE");
        Environment.SetEnvironmentVariable("IMS_AWS_S3_SECRET_KEY", "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY");
        Environment.SetEnvironmentVariable("IMS_AWS_S3_BUCKETNAME", "91dev-ap-northeast-1-private-tw-ims");
        Environment.SetEnvironmentVariable("TP_AWS_S3_BUCKETNAME", "91dev-ap-northeast-1-private-tw-ims");
        Environment.SetEnvironmentVariable("IMS_AWS_S3_STATIC_RESOURCE_PUBLIC_DNS", "https://ims-static.qa.91dev.tw");
        Environment.SetEnvironmentVariable("IMS_NINE1_NMQ_CLIENT_API_KEY","123456");
    }

    public static void SetMockServerUrlAfterInitial()
    {
        Environment.SetEnvironmentVariable("IMS_API_DOMAIN",MockServerHelper.Hostname);
        Environment.SetEnvironmentVariable("IMS_PLUS_API_DOMAIN", MockServerHelper.Hostname);
    }

    public static void SetRedisDomainUrlAfterInitial(string redisDomainUrl) 
    {
        Environment.SetEnvironmentVariable("IMS_REDIS_DOMAIN", redisDomainUrl);
    }
    
    public static string GetDbConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("IMS_DATABASE_HOST");
        var userName = Environment.GetEnvironmentVariable("IMS_DATABASE_USER");
        var password = Environment.GetEnvironmentVariable("IMS_DATABASE_PASSWORD");
        var dbName = Environment.GetEnvironmentVariable("IMS_DATABASE_NAME");

        return $"Host={host};Username={userName};Password={password};Database={dbName}";
    }

    public static (string Host, string UserName, string Password, string DatabaseName) GetDbConnectionConfig()
    {
        var host = Environment.GetEnvironmentVariable("IMS_DATABASE_HOST");
        var userName = Environment.GetEnvironmentVariable("IMS_DATABASE_USER");
        var password = Environment.GetEnvironmentVariable("IMS_DATABASE_PASSWORD");
        var dbName = Environment.GetEnvironmentVariable("IMS_DATABASE_NAME");

        return (host, userName, password, dbName);
    }
    
    public static Dictionary<string, string> SetInMemoryConfigVariables()
    {
        SetEnvironmentVariables();
        return new Dictionary<string, string>
        {
            { "_N1SECRET:IMSDB.Host", Environment.GetEnvironmentVariable("IMS_DATABASE_HOST") },
            { "_N1SECRET:IMSDB.UserName", Environment.GetEnvironmentVariable("IMS_DATABASE_USER") },
            { "_N1SECRET:IMSDB.Password", Environment.GetEnvironmentVariable("IMS_DATABASE_PASSWORD") },
            { "_N1SECRET:IMSDB.DBName", Environment.GetEnvironmentVariable("IMS_DATABASE_NAME") },
        };
    }
    
    public static DateTime ToUtc(string time)
    {
        var tempTime = DateTimeOffset.Parse(time);
        var utcTime = new DateTimeOffset(tempTime.DateTime, TimeSpan.Zero).UtcDateTime;
        return utcTime;
    }
}