using Xunit;
using PeasyPilot.Core.Testing;

namespace PeasyPilot.XUnit;

/// <summary>
/// Base class for xUnit test classes integrating with PeasyPilot.
/// Includes centralized logging that outputs to console, visible in CI/CD.
/// </summary>
public abstract class PeasyPilotTestBase : TestLoggerBase, IAsyncLifetime
{
    /// <summary>
    /// Initializes the test fixture asynchronously with logging.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task InitializeAsync()
    {
        await InitializeLoggerAsync();
    }

    /// <summary>
    /// Disposes the test fixture asynchronously with logging.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DisposeAsync()
    {
        await DisposeLoggerAsync();
    }
}
