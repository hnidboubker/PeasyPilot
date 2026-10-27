namespace PeasyPilot.Core.Tests.CLI;

using PeasyPilot.CLI;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Scheduling;
using PeasyPilot.Core.Storage;
using Xunit;

public class CliRunnerTests
{
    [Fact]
    public async Task RunAsync_HelpOption_ReturnsZeroExitCode()
    {
        var exitCode = await CliRunner.RunAsync(["--help"]);
        PeasyAssert.Equal(0, exitCode);
    }

    [Fact]
    public async Task RunAsync_NoAssemblyAndNoOverride_RefusesToReportPassed()
    {
        var store = new FileTestRunStore(Path.Combine(Path.GetTempPath(), "peasypilot-tests", Guid.NewGuid().ToString("N")));

        var exitCode = await CliRunner.RunAsync([], storeOverride: store);

        PeasyAssert.Equal(2, exitCode);
        PeasyAssert.IsEmpty(await store.GetRunHistoryAsync(limit: 10));
    }

    [Fact]
    public async Task RunAsync_HistoryCommand_DisplaysHistoryAndReturnsZero()
    {
        var store = new InMemoryTestRunStore();
        await store.SaveRunAsync(new TestRunRecord
        {
            PipelineResult = new TestPipelineResult
            {
                AggregateRunResult = new TestRunResult { Passed = 2, Status = TestRunStatus.Passed }
            }
        });

        var exitCode = await CliRunner.RunAsync(["history"], storeOverride: store);
        PeasyAssert.Equal(0, exitCode);
    }

    [Fact]
    public async Task RunAsync_PipelineExecution_Success_ReturnsZeroExitCode()
    {
        var discovery = new DefaultTestDiscovery(
        [
            new TestCase { Name = "Customer.Register", Category = "unit" },
            new TestCase { Name = "Invoice.Create", Category = "unit" }
        ]);

        var scheduler = new DefaultTestScheduler();
        var store = new InMemoryTestRunStore();

        var exitCode = await CliRunner.RunAsync(
            ["--filter", "Customer"],
            discoveryOverride: discovery,
            schedulerOverride: scheduler,
            storeOverride: store);

        PeasyAssert.Equal(0, exitCode);

        var history = await store.GetRunHistoryAsync();
        PeasyAssert.Single(history);
        PeasyAssert.Equal(1, history.First().PipelineResult.ScheduledCount);
    }

    [Fact]
    public async Task RunAsync_PipelineExecution_WithChangedFiles_FiltersByImpact()
    {
        var discovery = new DefaultTestDiscovery(
        [
            new TestCase { Name = "CustomerService.Create", Category = "unit" },
            new TestCase { Name = "OrderService.Process", Category = "unit" }
        ]);

        var scheduler = new DefaultTestScheduler();
        var store = new InMemoryTestRunStore();

        var exitCode = await CliRunner.RunAsync(
            ["--changed-files", "CustomerService"],
            discoveryOverride: discovery,
            schedulerOverride: scheduler,
            storeOverride: store);

        PeasyAssert.Equal(0, exitCode);

        var history = await store.GetRunHistoryAsync();
        PeasyAssert.Single(history);
        PeasyAssert.Equal(1, history.First().PipelineResult.ImpactedCount);
    }
}
