using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Reporting;
using Xunit;

namespace PeasyPilot.Core.Tests.Reporting;

public class JsonFileReporterTests
{
    [Fact]
    public async Task ReportAsync_WithTests_IncludesEachTestNameAndMessage()
    {
        var run = new TestRunResult
        {
            Failed = 1,
            Status = TestRunStatus.Failed,
            Tests = [new() { Name = "Shop.Orders.Divides", Status = TestRunStatus.Failed, Message = "Expected 2 but was 3" }]
        };

        var json = await new JsonFileReporter().ReportAsync(run);

        PeasyAssert.Contains("Shop.Orders.Divides", json);
        PeasyAssert.Contains("Expected 2 but was 3", json);
    }
}
