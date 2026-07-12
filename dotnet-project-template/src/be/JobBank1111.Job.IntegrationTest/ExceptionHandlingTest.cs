using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

/// <summary>
/// ExceptionHandlingMiddleware 環境閘門驗證（health-check 2026-07-12 D5）。
/// TestServer 以 "Testing" 環境啟動（非 Development），
/// 未處理例外的回應不得包含例外原文，但須保留 TraceId。
/// </summary>
public class ExceptionHandlingTest
{
    [Fact]
    public async Task 非Development環境_未處理例外_不回傳例外原文()
    {
        TestAssistant.SetDummyEnvironmentVariablesIfMissing();

        using var server = new TestServer(DateTimeOffset.UtcNow, "exception-handling-test");
        using var client = server.CreateClient();

        var response = await client.GetAsync("/api/v1/tests:throw");

        ((int)response.StatusCode).Should().Be(500);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonNode.Parse(body)!.AsObject();

        var message = json["message"]?.GetValue<string>();
        message.Should().NotBeNullOrWhiteSpace();
        message.Should().NotContain("敏感內部錯誤", "非 Development 環境不得回傳例外原文");
        message.Should().NotContain("Password", "非 Development 環境不得洩漏連線字串等內部細節");

        json["traceId"]?.GetValue<string>().Should().NotBeNullOrWhiteSpace("TraceId 須保留供關聯 server log");
        json["code"]?.GetValue<string>().Should().Be("Unknown");
    }
}
