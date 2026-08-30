namespace PeasyPilot.Core.Tests.Discovery;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Eums;
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

        Assert.Equal(2, tests.Count);
        Assert.Equal("Customer.Register", tests.First().Name);
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

        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.Equal(TestRunStatus.Passed, result.Status));
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

        Assert.Equal(2, impacted.Count);
        Assert.All(impacted, test => Assert.Contains("Customer", test.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Contracts_ShouldExistForDiscoveryAndScheduler()
    {
        Assert.True(typeof(ITestDiscovery).IsInterface);
        Assert.True(typeof(ITestScheduler).IsInterface);
        Assert.True(typeof(ITestImpactAnalyzer).IsInterface);
    }
}
