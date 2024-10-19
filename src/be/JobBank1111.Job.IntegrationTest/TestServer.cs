using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

public class TestServer : WebApplicationFactory<Program>
{
    private void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<FakeTimeProvider>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(this.ConfigureServices);
    }
}