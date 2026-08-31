namespace PeasyPilot.Core.Abstractions;

using PeasyPilot.Core.Models;

/// <summary>
/// Discovers tests in a framework-agnostic way.
/// </summary>
public interface ITestDiscovery
{
    /// <summary>
    /// Discovers the available tests.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The discovered tests.</returns>
    Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default);
}
