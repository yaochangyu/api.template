using System.Net;
using System.Text;
using Reqnroll;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

public static class ScenarioContextExtension
{
    public static void SetServiceProvider(this ScenarioContext scenarioContext, IServiceProvider serviceProvider)
    {
        scenarioContext.Set(serviceProvider);
    }

    public static IServiceProvider GetServiceProvider(this ScenarioContext scenarioContext)
    {
        return scenarioContext.Get<IServiceProvider>();
    }

    public static string? GetMarket(this ScenarioContext context)
        => context.TryGetValue($"Market", out string market)
            ? market
            : null;

    public static void SetMarket(this ScenarioContext context, string market)
        => context.Set(market, $"Market");

    public static string? GetUserId(this ScenarioContext context)
        => context.TryGetValue($"UserId", out string userId)
            ? userId
            : null;

    public static void SetUserId(this ScenarioContext context, string userId) => context.Set(userId, $"UserId");

    public static DateTimeOffset? GetUtcNow(this ScenarioContext context) =>
        context.TryGetValue($"UtcNow", out DateTimeOffset dateTime)
            ? dateTime
            : null;

    public static void SetUtcNow(this ScenarioContext context, DateTimeOffset? dateTime) =>
        context.Set(dateTime, $"UtcNow");

    public static long? GetFirmId(this ScenarioContext context) =>
        context.TryGetValue($"FirmId", out long firmId)
            ? firmId
            : null;

    public static void SetFirmId(this ScenarioContext context, long firmId) =>
        context.Set(firmId, $"FirmId");

    public static void SetHttpClient(this ScenarioContext scenarioContext, HttpClient httpClient) =>
        scenarioContext.Set(httpClient);

    public static HttpClient GetHttpClient(this ScenarioContext scenarioContext) =>
        scenarioContext.Get<HttpClient>();

    public static void AddQueryString(this ScenarioContext context, string key, string value)
    {
        if (!context.TryGetValue<IList<(string Key, string Value)>>("QueryString", out var data))
        {
            data = new List<(string Key, string Value)>();
        }

        data.Add((key, value));
        context.Set(data, "QueryString");
    }

    public static HttpResponseMessage GetHttpResponse(this ScenarioContext context) =>
        context.TryGetValue(out HttpResponseMessage result) ? result : default;

    public static void SetHttpResponseBody(this ScenarioContext context, string body) =>
        context.Set(body, "HttpResponseBody");

    public static string GetHttpResponseBody(this ScenarioContext context) =>
        context.TryGetValue("HttpResponseBody", out string body) ? body : null;

    public static void SetHttpStatusCode(this ScenarioContext scenarioContext, HttpStatusCode httpStatusCode) =>
        scenarioContext.Set(httpStatusCode, "HttpStatusCode");

    public static HttpStatusCode GetHttpStatusCode(this ScenarioContext scenarioContext) =>
        scenarioContext.Get<HttpStatusCode>("HttpStatusCode");

    public static void SetXUnitLog(this ScenarioContext context, StringBuilder stringBuilder)
    {
        context.Set(stringBuilder, "XUnitLog");
    }

    public static StringBuilder GetXUnitLog(this ScenarioContext context)
    {
        context.TryGetValue("XUnitLog", out StringBuilder? stringBuilder);
        return stringBuilder ?? new StringBuilder();
    }
}