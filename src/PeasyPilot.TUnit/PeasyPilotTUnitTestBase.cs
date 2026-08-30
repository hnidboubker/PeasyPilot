using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;
namespace PeasyPilot.TUnit;
/// <summary>
/// Base class for TUnit test classes integrating with PeasyPilot.
/// </summary>
public abstract class PeasyPilotTUnitTestBase
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
    /// Initializes the test.
    /// </summary>
    public virtual ValueTask BeforeEachAsync()
    {
        TestContext = new TestContext();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Cleans up after the test.
    /// </summary>
    public virtual ValueTask AfterEachAsync()
    {
        return ValueTask.CompletedTask;
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
