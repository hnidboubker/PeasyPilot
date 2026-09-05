using Xunit;
using PeasyPilot.Core;

namespace PeasyPilot.XUnit;

/// <summary>
/// Base class for xUnit test classes integrating with PeasyPilot.
/// Implements xUnit's IAsyncLifetime for async setup/teardown.
/// </summary>
public abstract class PeasyPilotTestBase : Core.PeasyPilotTestBase, IAsyncLifetime
{
    /// <summary>
    /// Initializes the test fixture asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task InitializeAsync()
    {
        InitializeContext();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the test fixture asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DisposeAsync()
    {
        CleanupContext();
        await Task.CompletedTask;
    }
}
