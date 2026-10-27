namespace PeasyPilot.Core.Tests.Discovery;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.ImpactAnalysis;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Scheduling;
using Xunit;

public class DiscoverySchedulerImpactTests
{
    [Fact]
    public async Task Discovery_ShouldReturnTestsFromProvider()
    {
        var discovery = new DefaultTestDiscovery(
        [
            new TestCase { Name = "Customer.Register", Category = "unit" },
            new TestCase { Name = "Invoice.Create", Category = "unit" }
        ]);

        var tests = await discovery.DiscoverAsync();

        PeasyAssert.Equal(2, tests.Count);
        PeasyAssert.Equal("Customer.Register", tests.First().Name);
    }

    [Fact]
    public async Task Scheduler_ShouldExecuteTestsAndReturnResults()
    {
        var scheduler = new DefaultTestScheduler();
        var tests = new[]
        {
            new TestCase { Name = "Customer.Register", Category = "unit" },
            new TestCase { Name = "Invoice.Create", Category = "unit" }
        };

        var results = await scheduler.ExecuteAsync(tests);

        PeasyAssert.Equal(2, results.Count);
        PeasyAssert.All(results, result => PeasyAssert.Equal(TestRunStatus.Passed, result.Status));
    }

    [Fact]
    public async Task ImpactAnalyzer_ShouldReturnMatchingTestsFromChangedFiles()
    {
        var analyzer = new DefaultTestImpactAnalyzer();
        var tests = new[]
        {
            new TestCase { Name = "Customer.Register", Category = "customer" },
            new TestCase { Name = "Invoice.Create", Category = "billing" },
            new TestCase { Name = "Customer.Update", Category = "customer" }
        };

        var impacted = await analyzer.GetImpactedTestsAsync(["Customer"], tests);

        PeasyAssert.Equal(2, impacted.Count);
        PeasyAssert.All(impacted, test => PeasyAssert.Contains("Customer", test.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Contracts_ShouldExistForDiscoveryAndScheduler()
    {
        PeasyAssert.True(typeof(ITestDiscovery).IsInterface);
        PeasyAssert.True(typeof(ITestScheduler).IsInterface);
        PeasyAssert.True(typeof(ITestImpactAnalyzer).IsInterface);
    }
}
