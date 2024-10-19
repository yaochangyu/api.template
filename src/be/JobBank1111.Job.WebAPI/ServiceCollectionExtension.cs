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
        services.AddSingleton<ContextAccessor<AuthContext>>();
        services.AddSingleton<IContextGetter<AuthContext>>(p => p.GetService<ContextAccessor<AuthContext>>());
        services.AddSingleton<IContextSetter<AuthContext>>(p => p.GetService<ContextAccessor<AuthContext>>());
        return services;
    }
}