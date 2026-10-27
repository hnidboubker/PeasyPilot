namespace PeasyPilot.Core.Tests.Reporting;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Reporting;
using Xunit;

public class ReportingDiagnosticsTests
{
    [Fact]
    public async Task ConsoleReporter_ShouldWriteSummary()
    {
        var result = new TestRunResult
        {
            Passed = 2,
            Failed = 1,
            Skipped = 0,
            Duration = TimeSpan.FromSeconds(1),
            Status = PeasyPilot.Core.Eums.TestRunStatus.Failed
        };

        var reporter = new ConsoleReporter();
        var output = await reporter.ReportAsync(result);

        PeasyAssert.NotNull(output);
        PeasyAssert.Contains("2", output);
    }

    [Fact]
    public void TestDiagnosticResult_ShouldCaptureSummaryAndSuggestions()
    {
        var diagnostic = new TestDiagnosticResult(
            "The test failed because the object was null.",
            "A null dependency was not initialized.",
            ["CustomerServiceTests", "OrderServiceTests"],
            ["Initialize dependency before execution."]);

        PeasyAssert.Equal("The test failed because the object was null.", diagnostic.Summary);
        PeasyAssert.NotEmpty(diagnostic.Suggestions);
    }

    [Fact]
    public void ReporterAndDiagnosticContracts_ShouldBeCompatible()
    {
        PeasyAssert.True(typeof(ITestReporter).IsInterface);
        PeasyAssert.True(typeof(ITestDiagnostic).IsInterface);
    }
}
