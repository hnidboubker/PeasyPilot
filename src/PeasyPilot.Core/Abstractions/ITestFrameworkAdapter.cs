using PeasyPilot.Core.Models;
namespace PeasyPilot.Core.Abstractions;
/// <summary>
/// Adapts a concrete test framework to the shared PeasyPilot model.
/// </summary>
public interface ITestFrameworkAdapter
{
    /// <summary>
    /// Gets the adapter name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Discovers all tests supported by the target framework.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered tests.</returns>
    Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a request through the underlying framework adapter.
    /// </summary>
    /// <param name="request">The request to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregated execution result.</returns>
    Task<TestRunResult> RunAsync(TestRunRequest request, CancellationToken cancellationToken = default);
}
