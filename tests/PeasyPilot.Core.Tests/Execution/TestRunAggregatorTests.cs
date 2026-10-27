using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Execution;
using PeasyPilot.Core.Models;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.Execution;

public class TestRunAggregatorTests
{
    [Fact]
    public void Merge_TwoRuns_AddsTotalsAndDurations()
    {
        var xunit = new TestRunResult { Passed = 2, Duration = TimeSpan.FromSeconds(1), Status = TestRunStatus.Passed };
        var tunit = new TestRunResult { Passed = 1, Skipped = 1, Duration = TimeSpan.FromSeconds(2), Status = TestRunStatus.Passed };

        var merged = TestRunAggregator.Merge([xunit, tunit]);

        XAssert.Equal(3, merged.Passed);
        XAssert.Equal(1, merged.Skipped);
        XAssert.Equal(TimeSpan.FromSeconds(3), merged.Duration);
    }

    [Fact]
    public void Merge_TwoRuns_ConcatenatesTests()
    {
        var first = new TestRunResult { Passed = 1, Tests = [new() { Name = "A.One", Status = TestRunStatus.Passed }] };
        var second = new TestRunResult { Failed = 1, Tests = [new() { Name = "B.Two", Status = TestRunStatus.Failed }] };

        var merged = TestRunAggregator.Merge([first, second]);

        XAssert.Equal(2, merged.Tests.Count);
    }

    [Fact]
    public void Merge_AnyFailingRun_IsFailed()
    {
        var passing = new TestRunResult { Passed = 3, Status = TestRunStatus.Passed };
        var failing = new TestRunResult { Failed = 1, Status = TestRunStatus.Failed };

        var merged = TestRunAggregator.Merge([passing, failing]);

        XAssert.Equal(TestRunStatus.Failed, merged.Status);
    }

    [Fact]
    public void Merge_NoTests_IsNotStarted()
    {
        var merged = TestRunAggregator.Merge([]);

        XAssert.Equal(TestRunStatus.NotStarted, merged.Status);
    }
}
