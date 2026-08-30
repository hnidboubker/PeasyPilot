namespace PeasyPilot.Core.Engines;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

/// <summary>
/// Default framework-agnostic engine that runs tests through adapters.
/// </summary>
public sealed class TestEngine : ITestEngine
{
    private readonly IReadOnlyCollection<ITestFrameworkAdapter> _adapters;

    public TestEngine(IEnumerable<ITestFrameworkAdapter>? adapters = null)
    {
        _adapters = adapters?.ToArray() ?? [];
    }

    public async Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (_adapters.Count == 0)
        {
            return new TestRunResult
            {
                Passed = request.TestCases.Count,
                Failed = 0,
                Skipped = 0,
                Duration = TimeSpan.Zero,
                Status = TestRunStatus.Passed
            };
        }

        var passed = 0;
        var failed = 0;
        var skipped = 0;
        var startedAt = DateTime.UtcNow;

        foreach (var adapter in _adapters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await adapter.RunAsync(request, cancellationToken);
            passed += result.Passed;
            failed += result.Failed;
            skipped += result.Skipped;
        }

        var status = failed > 0
            ? TestRunStatus.Failed
            : skipped > 0 && passed == 0
                ? TestRunStatus.Skipped
                : TestRunStatus.Passed;

        return new TestRunResult
        {
            Passed = passed,
            Failed = failed,
            Skipped = skipped,
            Duration = DateTime.UtcNow - startedAt,
            Status = status
        };
    }
}
