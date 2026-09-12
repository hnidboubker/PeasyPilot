using PeasyPilot.Core.Testing;

namespace PeasyPilot.NUnit;

/// <summary>
/// Base class for NUnit test classes integrating with PeasyPilot.
/// Includes centralized logging that outputs to console, visible in CI/CD.
/// </summary>
public abstract class PeasyPilotNUnitTestBase : TestLoggerBase
{
    /// <summary>
    /// Setup called before each test with logging initialization.
    /// </summary>
    [global::NUnit.Framework.SetUp]
    public virtual void Setup()
    {
        InitializeLoggerAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Teardown called after each test with logging cleanup.
    /// </summary>
    [global::NUnit.Framework.TearDown]
    public virtual void TearDown()
    {
        DisposeLoggerAsync().GetAwaiter().GetResult();
    }
}
