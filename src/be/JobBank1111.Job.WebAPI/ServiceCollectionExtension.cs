using JobBank1111.Infrastructure.TraceContext;

namespace JobBank1111.Job.WebAPI;

public static class ServiceCollectionExtension
{
    public static IServiceCollection SetEnvironments(this IServiceCollection services)
    {
        services.AddSingleton<SYS_DATABASE_CONNECTION_STRING>();
        return services;
    }
    
    public static IServiceCollection SetContextAccessor(this IServiceCollection services)
    {
        services.AddSingleton<ContextAccessor<TraceContext>>();
        services.AddSingleton<IContextGetter<TraceContext>>(p => p.GetService<ContextAccessor<TraceContext>>());
        services.AddSingleton<IContextSetter<TraceContext>>(p => p.GetService<ContextAccessor<TraceContext>>());
        return services;
    }
}