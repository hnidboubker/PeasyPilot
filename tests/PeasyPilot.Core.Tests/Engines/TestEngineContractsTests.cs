namespace PeasyPilot.Core.Tests.Engines;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;
using Xunit;

public class TestEngineContractsTests
{
    [Fact]
    public void TestRunRequest_ShouldCarryMetadataAndCases()
    {
        var request = new TestRunRequest
        {
            Name = "Regression suite",
            Metadata = new Dictionary<string, string>
            {
                ["target"] = "net8.0"
            },
            TestCases =
            [
                new TestCase { Name = "User creation", Category = "unit" },
                new TestCase { Name = "Order total", Category = "unit" }
            ]
        };

        Assert.Equal("Regression suite", request.Name);
        Assert.Equal("net8.0", request.Metadata["target"]);
        Assert.Equal(2, request.TestCases.Count);
    }

    [Fact]
    public void TestRunResult_ShouldSummarizeExecutionOutcome()
    {
        var result = new TestRunResult
        {
            Passed = 3,
            Failed = 1,
            Skipped = 1,
            Duration = TimeSpan.FromSeconds(7.5),
            Status = TestRunStatus.Passed
        };

        Assert.Equal(3, result.Passed);
        Assert.Equal(1, result.Failed);
        Assert.Equal(1, result.Skipped);
        Assert.Equal(TestRunStatus.Passed, result.Status);
    }
}
