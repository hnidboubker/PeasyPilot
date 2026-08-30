namespace PeasyPilot.Core.Discovery;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

/// <summary>
/// Default in-memory implementation of test discovery.
/// </summary>
public sealed class DefaultTestDiscovery : ITestDiscovery
{
    private readonly IReadOnlyCollection<TestCase> _tests;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTestDiscovery"/> class.
    /// </summary>
    /// <param name="tests">The available tests.</param>
    public DefaultTestDiscovery(IEnumerable<TestCase>? tests = null)
    {
        _tests = tests?.ToArray() ?? [];
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<TestCase>> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_tests);
    }
}
