namespace JobBank1111.Job.WebAPI;

public static class HttpClientExtensions
{
    public static IHttpClientBuilder AddExternalApiHttpClient(this IServiceCollection services)
    {
        return services.AddHttpClient("externalApi",
                                      (provider, client) =>
                                      {
                                          var traceContext = provider.GetService<TraceContext>();
                                          var externalApi = provider.GetService<EXTERNAL_API>();
                                          var traceId = traceContext.TraceId;
                                          client.BaseAddress = new Uri(externalApi.Value);
                                          client.DefaultRequestHeaders.Add(SysHeaderNames.TraceId, traceId);
                                      })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
            {
                // 改成 true，會快取 Cookie
                UseCookies = false,
            });
    }
}