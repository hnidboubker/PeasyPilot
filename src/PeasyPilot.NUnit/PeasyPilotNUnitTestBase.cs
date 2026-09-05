using PeasyPilot.Core;

namespace PeasyPilot.NUnit;

/// <summary>
/// Base class for NUnit test classes integrating with PeasyPilot.
/// Implements NUnit's [SetUp]/[TearDown] lifecycle hooks.
/// </summary>
public abstract class PeasyPilotNUnitTestBase : Core.PeasyPilotTestBase
{
    /// <summary>
    /// Setup called before each test.
    /// </summary>
    [global::NUnit.Framework.SetUp]
    public virtual void Setup()
    {
        InitializeContext();
    }

    /// <summary>
    /// Teardown called after each test.
    /// </summary>
    [global::NUnit.Framework.TearDown]
    public virtual void TearDown()
    {
        CleanupContext();
    }
}
