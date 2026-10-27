using PeasyPilot.Core.Execution;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.Execution;

public class DotnetTestRunnerTests
{
    [Fact]
    public void BuildArguments_WithoutFramework_WritesTrxIntoResultsDirectory()
    {
        var args = DotnetTestRunner.BuildArguments("tests/Shop.Tests.csproj", "results", null);

        XAssert.Equal(
            new[] { "test", "tests/Shop.Tests.csproj", "--logger", "trx;LogFileName=report.trx", "--results-directory", "results" },
            args);
    }

    [Fact]
    public void BuildArguments_WithFramework_AddsFrameworkOption()
    {
        var args = DotnetTestRunner.BuildArguments("tests/Shop.Tests.csproj", "results", "net9.0");

        XAssert.Equal("--framework", args[^2]);
        XAssert.Equal("net9.0", args[^1]);
    }

    [Fact]
    public async Task RunAsync_MissingProject_ThrowsFileNotFound()
    {
        await XAssert.ThrowsAsync<FileNotFoundException>(() => new DotnetTestRunner().RunAsync("does-not-exist.csproj"));
    }

    [Fact]
    public void TargetsSeveralFrameworks_MultiTargetProject_IsDetected()
    {
        XAssert.True(DotnetTestRunner.TargetsSeveralFrameworks("<Project><PropertyGroup><TargetFrameworks>net8.0;net9.0</TargetFrameworks></PropertyGroup></Project>"));
        XAssert.False(DotnetTestRunner.TargetsSeveralFrameworks("<Project><PropertyGroup><TargetFramework>net9.0</TargetFramework></PropertyGroup></Project>"));
    }
}
