namespace PeasyPilot.Core.Tests.Engines;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using Xunit;

public class TestEnginePhase2Tests
{
    [Fact]
    public async Task TestEngine_ShouldAggregateAdapterResults()
    {
        var engine = new TestEngine(new[] { new StubFrameworkAdapter() });
        var request = new TestRunRequest
        {
            Name = "phase-2-suite",
            TestCases =
            [
                new TestCase { Name = "alpha", Category = "unit" },
                new TestCase { Name = "beta", Category = "unit" }
            ]
        };

        var result = await engine.RunAsync(request);

        Assert.Equal(2, result.Passed);
        Assert.Equal(0, result.Failed);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(TestRunStatus.Passed, result.Status);
    }

    [Fact]
    public void NameTestFilter_ShouldMatchCaseInsensitive()
    {
        var filter = new NameTestFilter("customer");
        var test = new TestCase { Name = "CustomerRegistration" };

        Assert.True(filter.Matches(test));
    }

    private sealed class StubFrameworkAdapter : ITestFrameworkAdapter
    {
        public string Name => "stub";

        public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<TestCase>>([
                new TestCase { Name = "alpha", Category = "unit" },
                new TestCase { Name = "beta", Category = "unit" }
            ]);

        public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new TestRunResult
            {
                Passed = request.TestCases.Count,
                Failed = 0,
                Skipped = 0,
                Duration = TimeSpan.FromMilliseconds(15),
                Status = TestRunStatus.Passed
            });
    }
}
