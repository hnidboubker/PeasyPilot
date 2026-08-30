using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Scheduling;
/// <summary>
/// Default in-memory implementation of a test scheduler.
/// </summary>
public sealed class DefaultTestScheduler : ITestScheduler
{
    /// <inheritdoc />
    public Task<IReadOnlyCollection<TestResult>> ExecuteAsync(IEnumerable<TestCase> tests, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tests);
        cancellationToken.ThrowIfCancellationRequested();

        var results = tests
            .Select(test => new TestResult
            {
                Name = test.Name,
                Category = test.Category,
                Status = TestRunStatus.Passed,
                Duration = TimeSpan.Zero,
                Message = "Executed successfully."
            })
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<TestResult>>(results);
    }
}
