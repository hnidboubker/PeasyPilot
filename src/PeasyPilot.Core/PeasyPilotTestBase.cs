using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;

namespace PeasyPilot.Core;

/// <summary>
/// Common base class for all PeasyPilot test classes.
/// Provides access to test context, test data factory, and mock factory.
/// Framework-specific lifecycle hooks are implemented in derived classes.
/// </summary>
public abstract class PeasyPilotTestBase
{
    /// <summary>
    /// Gets the test context.
    /// </summary>
    protected ITestContext TestContext { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the test data factory.
    /// </summary>
    protected ITestDataFactory? TestDataFactory { get; set; }

    /// <summary>
    /// Gets or sets the mock factory.
    /// </summary>
    protected IMockFactory? MockFactory { get; set; }

    /// <summary>
    /// Initializes the test context. Called by framework-specific lifecycle hooks.
    /// </summary>
    protected virtual void InitializeContext()
    {
        TestContext = new TestContext();
    }

    /// <summary>
    /// Cleans up after the test. Called by framework-specific lifecycle hooks.
    /// </summary>
    protected virtual void CleanupContext()
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Gets or creates test data from the context.
    /// </summary>
    /// <typeparam name="T">The type of test data.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function.</param>
    /// <returns>The test data.</returns>
    protected T GetOrCreateTestData<T>(string key, Func<T> factory) where T : class
    {
        return TestContext.GetOrAdd(key, factory);
    }
}
