namespace PeasyPilot.Core.Tests.Storage;

using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Storage;
using Xunit;

public class TestRunStoreTests
{
    [Fact]
    public async Task InMemoryTestRunStore_SavesAndRetrievesHistory()
    {
        // Arrange
        var store = new InMemoryTestRunStore();
        var record1 = new TestRunRecord
        {
            ExecutedAt = DateTime.UtcNow.AddMinutes(-10),
            PipelineResult = new TestPipelineResult
            {
                DiscoveredCount = 5,
                ScheduledCount = 5,
                AggregateRunResult = new TestRunResult { Passed = 5, Status = TestRunStatus.Passed }
            }
        };

        var record2 = new TestRunRecord
        {
            ExecutedAt = DateTime.UtcNow,
            PipelineResult = new TestPipelineResult
            {
                DiscoveredCount = 5,
                ScheduledCount = 5,
                AggregateRunResult = new TestRunResult { Passed = 4, Failed = 1, Status = TestRunStatus.Failed }
            }
        };

        // Act
        await store.SaveRunAsync(record1);
        await store.SaveRunAsync(record2);

        var history = await store.GetRunHistoryAsync();
        var latest = await store.GetLatestRunAsync();

        // Assert
        Assert.Equal(2, history.Count);
        Assert.NotNull(latest);
        Assert.Equal(record2.RunId, latest.RunId);
        Assert.Equal(TestRunStatus.Failed, latest.PipelineResult.Status);
    }

    [Fact]
    public async Task FileTestRunStore_SavesAndReadsFromFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "peasypilot_tests_" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new FileTestRunStore(tempDir);
            var record = new TestRunRecord
            {
                ExecutedAt = DateTime.UtcNow,
                PipelineResult = new TestPipelineResult
                {
                    DiscoveredCount = 3,
                    ScheduledCount = 3,
                    AggregateRunResult = new TestRunResult { Passed = 3, Status = TestRunStatus.Passed }
                }
            };

            // Act
            await store.SaveRunAsync(record);
            var history = await store.GetRunHistoryAsync();
            var latest = await store.GetLatestRunAsync();

            // Assert
            Assert.Single(history);
            Assert.NotNull(latest);
            Assert.Equal(record.RunId, latest.RunId);
            Assert.Equal(3, latest.PipelineResult.DiscoveredCount);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
