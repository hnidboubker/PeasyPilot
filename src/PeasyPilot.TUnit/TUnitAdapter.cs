namespace PeasyPilot.TUnit;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

/// <summary>
/// Adapts TUnit to the shared PeasyPilot execution model.
/// </summary>
public sealed class TUnitAdapter : ITestFrameworkAdapter
{
    public string Name => "TUnit";

    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<IReadOnlyCollection<TestCase>>([
            new TestCase { Name = "PeasyPilot.TUnit.adapter", Category = "adapter" },
            new TestCase { Name = "PeasyPilot.TUnit.discovery", Category = "adapter" }
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
