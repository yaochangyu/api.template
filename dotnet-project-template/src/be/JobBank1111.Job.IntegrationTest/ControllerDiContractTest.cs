using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JobBank1111.Job.WebAPI.IntegrationTest;

/// <summary>
/// 啟動期 Controller DI 契約測試（health-check 2026-07-12 D8）。
/// MVC 預設不把 Controller 註冊進 DI Container，ValidateOnBuild 驗不到
/// Controller 建構子相依（P0-1 的 IMemberController 缺註冊就是這樣漏掉的），
/// 因此用反射列舉所有 Controller，逐一驗證建構子參數皆可解析。
/// </summary>
public class ControllerDiContractTest
{
    [Fact]
    public void 所有Controller建構子相依均可自DI解析()
    {
        // DI 解析不會真的連線，僅需環境變數存在
        TestAssistant.SetDummyEnvironmentVariablesIfMissing();

        using var server = new TestServer(DateTimeOffset.UtcNow, "di-contract-test");

        var controllerTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(type => type.IsAbstract == false
                           && typeof(ControllerBase).IsAssignableFrom(type))
            .OrderBy(type => type.FullName)
            .ToList();

        controllerTypes.Should().NotBeEmpty();

        var failures = new List<string>();
        foreach (var controllerType in controllerTypes)
        {
            using var scope = server.Services.CreateScope();
            try
            {
                ActivatorUtilities.CreateInstance(scope.ServiceProvider, controllerType);
            }
            catch (Exception ex)
            {
                failures.Add($"{controllerType.FullName}: {ex.Message}");
            }
        }

        failures.Should().BeEmpty(
            "以下 Controller 的建構子相依無法從 DI Container 解析，"
            + "請檢查 Program.cs 的服務註冊：\n" + string.Join("\n", failures));
    }
}
