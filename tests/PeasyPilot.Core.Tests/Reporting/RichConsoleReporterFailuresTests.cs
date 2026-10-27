using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Reporting;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.Reporting;

public class RichConsoleReporterFailuresTests
{
    [Fact]
    public async Task ReportAsync_WithFailedTests_ListsEachFailureWithItsMessage()
    {
        var run = new TestRunResult
        {
            Passed = 1,
            Failed = 1,
            Status = TestRunStatus.Failed,
            Tests =
            [
                new() { Name = "Shop.Orders.Adds", Status = TestRunStatus.Passed },
                new() { Name = "Shop.Orders.Divides", Status = TestRunStatus.Failed, Message = "Expected 2 but was 3" }
            ]
        };

        var output = await new RichConsoleReporter().ReportAsync(run);

        XAssert.Contains("Failed tests:", output);
        XAssert.Contains("Shop.Orders.Divides", output);
        XAssert.Contains("Expected 2 but was 3", output);
        XAssert.DoesNotContain("Shop.Orders.Adds", output);
    }

    [Fact]
    public async Task ReportAsync_WithoutFailedTests_OmitsFailureSection()
    {
        var run = new TestRunResult { Passed = 1, Status = TestRunStatus.Passed, Tests = [new() { Name = "A.B", Status = TestRunStatus.Passed }] };

        var output = await new RichConsoleReporter().ReportAsync(run);

        XAssert.DoesNotContain("Failed tests:", output);
    }
}
