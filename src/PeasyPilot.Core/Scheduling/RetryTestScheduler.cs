using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Scheduling;

/// <summary>
/// Scheduler wrapper that retries failed tests to detect and handle flaky tests.
/// </summary>
public sealed class RetryTestScheduler : ITestScheduler
{
    private readonly ITestScheduler _innerScheduler;
    private readonly int _maxRetries;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryTestScheduler"/> class.
    /// </summary>
    /// <param name="innerScheduler">The underlying test scheduler.</param>
    /// <param name="maxRetries">Maximum retry attempts for failed tests (default 2).</param>
    public RetryTestScheduler(ITestScheduler innerScheduler, int maxRetries = 2)
    {
        _innerScheduler = innerScheduler ?? throw new ArgumentNullException(nameof(innerScheduler));
        _maxRetries = maxRetries;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TestResult>> ExecuteAsync(IEnumerable<TestCase> tests, CancellationToken cancellationToken = default)
    {
        var testList = tests.ToList();
        var initialResults = await _innerScheduler.ExecuteAsync(testList, cancellationToken);

        var finalResults = new List<TestResult>();

        foreach (var result in initialResults)
        {
            if (result.Status == TestRunStatus.Failed && _maxRetries > 0)
            {
                var retriedResult = result;
                var matchingCase = testList.FirstOrDefault(t => t.Name == result.Name);

                if (matchingCase != null)
                {
                    for (var attempt = 1; attempt <= _maxRetries; attempt++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var retryBatch = await _innerScheduler.ExecuteAsync([matchingCase], cancellationToken);
                        var retryRes = retryBatch.FirstOrDefault();

                        if (retryRes != null && retryRes.Status == TestRunStatus.Passed)
                        {
                            retriedResult = new TestResult
                            {
                                Name = retryRes.Name,
                                Category = retryRes.Category,
                                Status = TestRunStatus.Passed,
                                Duration = retryRes.Duration,
                                Message = $"Flaky test passed on retry attempt {attempt}."
                            };
                            break;
                        }
                    }
                }

                finalResults.Add(retriedResult);
            }
            else
            {
                finalResults.Add(result);
            }
        }

        return finalResults;
    }
}
