using JobBank1111.Testing.Common;
using JobBank1111.Testing.Common.MockServer;
using JobBank1111.Testing.Common.MockServers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Xunit.Abstractions;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

[Binding]
public class BaseStep : Steps
{
    private readonly ITestOutputHelper _testOutputHelper;
    static HttpClient ExternalClient;

    public BaseStep(ITestOutputHelper testOutputHelper)
    {
        this._testOutputHelper = testOutputHelper;
    }

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        TestAssistant.SetEnvironmentVariables();

        //建立容器
        await CreateContainersAsync();

        //建立測試專案的 DI Containers
        var serviceProvider = CreateServiceProvider();
        
        // //初始化測試專案需要的資源
        // await InitialDatabase(serviceProvider);
        // await InitialS3(serviceProvider);
        //
        // async Task InitialDatabase(ServiceProvider serviceProvider)
        // {
        //     var dbContextFactory = serviceProvider.GetService<IDbContextFactory<FakeImsDbContext>>();
        //     await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        //     await dbContext.Initial();
        // }
        //
        // async Task InitialS3(ServiceProvider serviceProvider)
        // {
        //     var bucketName = Environment.GetEnvironmentVariable("IMS_AWS_S3_BUCKETNAME");
        //     var s3Client = serviceProvider.GetService<IAmazonS3>();
        //     try
        //     {
        //         // delete bucket
        //         await AmazonS3Util.DeleteS3BucketWithObjectsAsync(s3Client, bucketName);
        //     }
        //     catch (Exception e)
        //     {
        //     }
        //
        //     // create bucket
        //     var putBucketRequest = new PutBucketRequest
        //     {
        //         BucketName = bucketName,
        //         UseClientRegion = true
        //     };
        //
        //     await s3Client.PutBucketAsync(putBucketRequest);
        // }

        async Task CreateContainersAsync()
        {
            var msSqlContainer = await TestContainerFactory.CreateMsSqlContainerAsync();
            var dbConnectionString = msSqlContainer.GetConnectionString();
            TestAssistant.SetDbConnectionEnvironmentVariable(dbConnectionString);

            var redisContainer = await TestContainerFactory.CreateRedisContainerAsync();
            var redisDomainUrl = redisContainer.GetConnectionString();
            TestAssistant.SetRedisConnectionEnvironmentVariable(redisDomainUrl);

            var mockServerContainer = await TestContainerFactory.CreateMockServerContainerAsync();
            var externalUrl = TestContainerFactory.GetMockServerConnection(mockServerContainer);
            TestAssistant.SetExternalConnectionEnvironmentVariable(externalUrl);
            ExternalClient = new HttpClient() { BaseAddress = new Uri(externalUrl) };
        }
    }
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        // services.AddFakeDatabase(DbConnectionString);
        // services.AddFakeRedis(RedisDomainUrl);
        // services.AddFakeS3Client(S3Url);
        // services.AddFakeApiInformationService();
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
    [Given(@"資料庫已存在 Member 資料")]
    public void Given資料庫已存在Member資料(Table table)
    {
    }

    [Given(@"建立模擬 API - HttpMethod = ""(.*)""，URL = ""(.*)""，StatusCode = ""(.*)""，ResponseContent =")]
    public async Task Given建立模擬apiHttpMethodUrlStatusCodeResponseContent(
        string httpMethod, string url, int statusCode, string body)
    {
        var client = ExternalClient;
        await MockedServerAssistant.PutNewEndPointAsync(client, httpMethod, url, statusCode, body);
    }
}