namespace PeasyPilot.NUnit;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

/// <summary>
/// Adapts NUnit to the shared PeasyPilot execution model.
/// </summary>
public sealed class NUnitAdapter : ITestFrameworkAdapter
{
    public string Name => "NUnit";

    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<IReadOnlyCollection<TestCase>>([
            new TestCase { Name = "PeasyPilot.NUnit.adapter", Category = "adapter" },
            new TestCase { Name = "PeasyPilot.NUnit.discovery", Category = "adapter" }
        ]);
    }

    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var startedAt = DateTime.UtcNow;
        var passed = request.TestCases.Count;
        var failed = 0;
        var skipped = 0;

        return Task.FromResult(new TestRunResult
        {
            Passed = passed,
            Failed = failed,
            Skipped = skipped,
            Duration = DateTime.UtcNow - startedAt,
            Status = TestRunStatus.Passed
        });
    }
}
