namespace global::PeasyPilot.Core.Abstractions;

using global::PeasyPilot.Core.Models;

/// <summary>
/// Defines the contract for a test engine that can execute a request and return a unified result.
/// </summary>
public interface ITestEngine
{
    /// <summary>
    /// Executes the provided test run request.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The unified execution result.</returns>
    Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
