using JobBank1111.Infrastructure;
using JobBank1111.Job.WebAPI;
using JobBank1111.Job.WebAPI.Member;
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
    builder.Services.AddSingleton(p => JsonSerializeFactory.DefaultOptions);
    builder.Services.AddControllers()
        .AddJsonOptions(options => JsonSerializeFactory.Apply(options.JsonSerializerOptions))
        ;
    builder.Host
        .UseSerilog((context, services, config) =>
                        {
                            config.ReadFrom.Configuration(context.Configuration)
                                .ReadFrom.Services(services)
                                .Enrich.FromLogContext()
                                .WriteTo.Console();

                            if (!context.HostingEnvironment.IsEnvironment("Testing"))
                            {
                                config.WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                                    period: TimeSpan.FromSeconds(30), batchPostingLimit: 50);
                            }

                            config.WriteTo.File("logs/aspnet-.txt", rollingInterval: RollingInterval.Minute);
                        }
        );

    // 確定物件都有設定 DI Container
    builder.Host.UseDefaultServiceProvider(p =>
    {
        p.ValidateScopes = true;
        p.ValidateOnBuild = true;
    });
    var configuration = builder.Configuration;
    builder.Services.AddCacheProviderFactory(configuration);

    // OpenAPI support
    builder.Services.AddOpenApi();
    builder.Services.AddHttpContextAccessor();
    // Code First: MemberController - ASP.NET Core 自動發現，無需手動註冊
    builder.Services.AddScoped<JobBank1111.Job.WebAPI.Contract.IMemberV1Controller, JobBank1111.Job.WebAPI.Member.MemberV1ControllerImpl>();

    builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
    builder.Services.AddContextAccessor();
    builder.Services.AddSysEnvironments();
    builder.Services.AddScoped<IUuidProvider, UuidProvider>();
    builder.Services.AddScoped<MemberHandler>();
    builder.Services.AddScoped<MemberRepository>();
    builder.Services.AddExternalApiHttpClient();
    builder.Services.AddDatabase();
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseMiddleware<TraceContextMiddleware>();
    app.UseMiddleware<MeasurementMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestParameterLoggerMiddleware>();
    app.UseRouting();
    app.UseAuthorization();
    app.UseSerilogRequestLogging();

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