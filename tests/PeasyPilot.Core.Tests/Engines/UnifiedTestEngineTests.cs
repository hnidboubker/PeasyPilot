namespace PeasyPilot.Core.Tests.Engines;

using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Engines;
using PeasyPilot.Core.Models;
using Xunit;

public class UnifiedTestEngineTests
{
    [Fact]
    public async Task RunAsync_ShouldExecuteAllCases_AndReturnAggregateResult()
    {
        var engine = new UnifiedTestEngine();
        var request = new TestRunRequest
        {
            Name = "feature-suite",
            TestCases =
            [
                new TestCase { Name = "A", Category = "unit" },
                new TestCase { Name = "B", Category = "unit" },
                new TestCase { Name = "C", Category = "integration" }
            ]
        };

        var result = await engine.RunAsync(request);

        PeasyAssert.Equal("feature-suite", request.Name);
        PeasyAssert.Equal(3, result.Passed);
        PeasyAssert.Equal(0, result.Failed);
        PeasyAssert.Equal(0, result.Skipped);
        PeasyAssert.Equal(TestRunStatus.Passed, result.Status);
        PeasyAssert.True(result.Duration >= TimeSpan.Zero);
    }
}
