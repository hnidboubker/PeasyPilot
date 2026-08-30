namespace PeasyPilot.Core.Tests.CLI;

using PeasyPilot.CLI;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Scheduling;
using PeasyPilot.Core.Storage;
using Xunit;
using Assert = Xunit.Assert;

public class CliRunnerTests
{
    [Fact]
    public async Task RunAsync_HelpOption_ReturnsZeroExitCode()
    {
        var exitCode = await CliRunner.RunAsync(["--help"]);
        Assert.Equal(0, exitCode);
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
        Assert.Equal(0, exitCode);
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

        Assert.Equal(0, exitCode);

        var history = await store.GetRunHistoryAsync();
        Assert.Single(history);
        Assert.Equal(1, history.First().PipelineResult.ScheduledCount);
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

        Assert.Equal(0, exitCode);

        var history = await store.GetRunHistoryAsync();
        Assert.Single(history);
        Assert.Equal(1, history.First().PipelineResult.ImpactedCount);
    }
}
