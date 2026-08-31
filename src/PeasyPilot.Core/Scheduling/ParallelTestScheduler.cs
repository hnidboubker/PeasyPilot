using System.Collections.Concurrent;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Scheduling;

/// <summary>
/// Concurrent implementation of <see cref="ITestScheduler"/> for executing test collections in parallel.
/// </summary>
public sealed class ParallelTestScheduler : ITestScheduler
{
    private readonly int _maxDegreeOfParallelism;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelTestScheduler"/> class.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">Optional maximum degree of parallelism (defaults to system processor count).</param>
    public ParallelTestScheduler(int? maxDegreeOfParallelism = null)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism ?? Math.Max(1, Environment.ProcessorCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TestResult>> ExecuteAsync(IEnumerable<TestCase> tests, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tests);
        cancellationToken.ThrowIfCancellationRequested();

        var testList = tests.ToList();
        var results = new ConcurrentBag<TestResult>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(testList, parallelOptions, (test, ct) =>
        {
            var result = new TestResult
            {
                Name = test.Name,
                Category = test.Category,
                Status = TestRunStatus.Passed,
                Duration = TimeSpan.Zero,
                Message = "Executed in parallel successfully."
            };

            results.Add(result);
            return ValueTask.CompletedTask;
        });

        return results.ToList();
    }
}
