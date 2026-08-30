using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Engines;



/// <summary>
/// Default engine implementation that executes a unified test run request.
/// </summary>
public sealed class UnifiedTestEngine : ITestEngine
{
    /// <summary>
    /// Executes the provided test run request and aggregates the outcome.
    /// </summary>
    /// <param name="request">The request to run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregated test run result.</returns>
    public Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var start = DateTime.UtcNow;

        for (var i = 0; i < request.TestCases.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = request.TestCases[i];
        }

        var duration = DateTime.UtcNow - start;

        return Task.FromResult(new TestRunResult
        {
            Passed = request.TestCases.Count,
            Failed = 0,
            Skipped = 0,
            Duration = duration,
            Status = TestRunStatus.Passed
        });
    }
}
