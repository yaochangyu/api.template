using System.Diagnostics;
using JobBank1111.Infrastructure;
using JobBank1111.Job.DB;
using JobBank1111.Job.WebAPI;
using JobBank1111.Job.WebAPI.Member;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/host-.txt", rollingInterval: RollingInterval.Hour)
    .CreateLogger();
Log.Information("Starting web host");

try
{
    if (Array.FindIndex(args, x => x == "--local") >= 0)
    {
        var envFolder = EnvironmentUtility.FindParentFolder("env");
        EnvironmentUtility.ReadEnvironmentFile(envFolder, "local.env");
    }

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllers();

    builder.Host.UseSerilog((context, services, config) =>
                                config.ReadFrom.Configuration(context.Configuration)
                                    .ReadFrom.Services(services)
                                    .Enrich.FromLogContext()
                                    .WriteTo.Console()
                                    .WriteTo.Seq("http://localhost:5341")
                                    .WriteTo.File("logs/aspnet-.txt", rollingInterval: RollingInterval.Minute)
    );

    // 確定物件都有設定 DI Container
    builder.Host.UseDefaultServiceProvider(p =>
    {
        p.ValidateScopes = true;
        p.ValidateOnBuild = true;
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSingleton(TimeProvider.System)
        .SetContextAccessor()
        .SetEnvironments();
    builder.Services.AddDbContextFactory<MemberDbContext>((provider, builder) =>
    {
        var environment = provider.GetService<SYS_DATABASE_CONNECTION_STRING>();
        var connectionString = environment.Value;
        builder.UseSqlServer(connectionString);
    });
    builder.Services.AddScoped<MemberCommand>();
    builder.Services.AddScoped<MemberRepository>();
    builder.Services.AddExternalApiHttpClient();
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.UseAuthorization();
    app.UseMiddleware<TraceContextMiddleware>();
    app.MapControllers();
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

namespace JobBank1111.Job.WebAPI
{
    public partial class Program
    {
    }
}