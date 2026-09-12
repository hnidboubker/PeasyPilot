using PeasyPilot.Core.Testing;

namespace PeasyPilot.TUnit;

/// <summary>
/// Base class for TUnit test classes integrating with PeasyPilot.
/// Includes centralized logging that outputs to console, visible in CI/CD.
/// </summary>
public abstract class PeasyPilotTUnitTestBase : TestLoggerBase
{
    /// <summary>
    /// Initializes the test with logging.
    /// </summary>
    public virtual async ValueTask BeforeEachAsync()
    {
        await InitializeLoggerAsync();
    }

    /// <summary>
    /// Cleans up after the test with logging.
    /// </summary>
    public virtual async ValueTask AfterEachAsync()
    {
        await DisposeLoggerAsync();
    }
}
