namespace PeasyPilot.Core.Tests.Scheduling;

using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Scheduling;
using Xunit;
using PeasyPilot.XUnit;

public class ParallelSchedulerTests
{
    [Fact]
    public async Task ParallelTestScheduler_ExecutesAllTestsConcurrently()
    {
        // Arrange
        var scheduler = new ParallelTestScheduler(maxDegreeOfParallelism: 4);
        var tests = Enumerable.Range(1, 10).Select(i => new TestCase
        {
            Name = $"Test.{i}",
            Category = "unit",
            Kind = TestKind.Unit
        }).ToList();

        // Act
        var results = await scheduler.ExecuteAsync(tests);

        // Assert
        XAssert.Equal(10, results.Count);
        XAssert.All(results, r => XAssert.Equal(TestRunStatus.Passed, r.Status));
    }
}
