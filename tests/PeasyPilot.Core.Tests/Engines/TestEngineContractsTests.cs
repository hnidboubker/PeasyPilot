namespace PeasyPilot.Core.Tests.Engines;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Eums;
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

        PeasyAssert.Equal("Regression suite", request.Name);
        PeasyAssert.Equal("net8.0", request.Metadata["target"]);
        PeasyAssert.Equal(2, request.TestCases.Count);
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

        PeasyAssert.Equal(3, result.Passed);
        PeasyAssert.Equal(1, result.Failed);
        PeasyAssert.Equal(1, result.Skipped);
        PeasyAssert.Equal(TestRunStatus.Passed, result.Status);
    }
}
