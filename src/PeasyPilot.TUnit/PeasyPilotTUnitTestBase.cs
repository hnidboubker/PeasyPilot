using PeasyPilot.Core;

namespace PeasyPilot.TUnit;

/// <summary>
/// Base class for TUnit test classes integrating with PeasyPilot.
/// Implements TUnit's BeforeEachAsync/AfterEachAsync lifecycle hooks.
/// </summary>
public abstract class PeasyPilotTUnitTestBase : Core.PeasyPilotTestBase
{
    /// <summary>
    /// Initializes the test.
    /// </summary>
    public virtual ValueTask BeforeEachAsync()
    {
        InitializeContext();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Cleans up after the test.
    /// </summary>
    public virtual ValueTask AfterEachAsync()
    {
        CleanupContext();
        return ValueTask.CompletedTask;
    }
}
