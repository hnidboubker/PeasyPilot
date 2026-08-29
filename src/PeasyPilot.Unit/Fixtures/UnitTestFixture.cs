namespace PeasyPilot.Unit.Fixtures;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;

/// <summary>
/// Base fixture for unit tests providing common test setup and utilities.
/// </summary>
public abstract class UnitTestFixture
{
    /// <summary>
    /// Gets the test context for this fixture.
    /// </summary>
    protected ITestContext TestContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitTestFixture"/> class.
    /// </summary>
    protected UnitTestFixture()
    {
        TestContext = new TestContext();
    }

    /// <summary>
    /// Gets or creates a value in the test context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value.</param>
    /// <returns>The cached or newly created value.</returns>
    protected T GetOrCreateTestData<T>(string key, Func<T> factory) where T : class
    {
        return TestContext.GetOrAdd(key, factory);
    }
}
