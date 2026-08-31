namespace PeasyPilot.Core.Tests.Engines;

using PeasyPilot.Core.Eums;
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

        Assert.Equal("feature-suite", request.Name);
        Assert.Equal(3, result.Passed);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(TestRunStatus.Passed, result.Status);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }
}
