using Xunit;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// Integration test fixture for xUnit framework.
/// Handles async initialization and cleanup automatically.
/// </summary>
public abstract class XUnitIntegrationTestFixture : IntegrationTestFixture, IAsyncLifetime
{
    /// <summary>
    /// xUnit calls this automatically before each test.
    /// </summary>
    async Task IAsyncLifetime.InitializeAsync() => await InitializeAsync();

    /// <summary>
    /// xUnit calls this automatically after each test.
    /// </summary>
    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}
