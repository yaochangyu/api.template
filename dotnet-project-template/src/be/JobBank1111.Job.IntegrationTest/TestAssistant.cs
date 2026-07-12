namespace JobBank1111.Job.WebAPI.IntegrationTest;

class TestAssistant
{
    public static void SetEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("JOB1111_ENVIRONMENT", "QA");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable(nameof(EXTERNAL_API), "http://localhost:5000/api");
    }

    public static void SetDbConnectionEnvironmentVariable(string connectionString)
    {
        Environment.SetEnvironmentVariable(nameof(SYS_DATABASE_CONNECTION_STRING), connectionString);
    }

    public static void SetRedisConnectionEnvironmentVariable(string url)
    {
        Environment.SetEnvironmentVariable(nameof(SYS_REDIS_URL), url);
    }

    public static void SetExternalConnectionEnvironmentVariable(string url)
    {
        Environment.SetEnvironmentVariable(nameof(EXTERNAL_API), url);
    }

    /// <summary>
    /// 不需要真實外部資源的測試用：只在缺值時補假連線字串，
    /// 避免蓋掉 BDD 測試由 Testcontainers 設定的真實連線字串
    /// </summary>
    public static void SetDummyEnvironmentVariablesIfMissing()
    {
        SetIfMissing(
            nameof(SYS_DATABASE_CONNECTION_STRING),
            "Server=localhost,1433;Database=dummy-test;User Id=sa;Password=dummy;TrustServerCertificate=True");
        SetIfMissing(nameof(SYS_REDIS_URL), "localhost:6379,abortConnect=false");
        SetIfMissing(nameof(EXTERNAL_API), "http://localhost:5000/api");

        static void SetIfMissing(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    public static DateTime ToUtc(string time)
    {
        var tempTime = DateTimeOffset.Parse(time);
        var utcTime = new DateTimeOffset(tempTime.DateTime, TimeSpan.Zero).UtcDateTime;
        return utcTime;
    }
}