using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;
namespace PeasyPilot.NUnit;

/// <summary>
/// Base class for NUnit test classes integrating with PeasyPilot.
/// </summary>
public abstract class PeasyPilotNUnitTestBase
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
    /// Setup called before each test.
    /// </summary>
    [global::NUnit.Framework.SetUp]
    public virtual void Setup()
    {
        TestContext = new TestContext();
    }

    /// <summary>
    /// Teardown called after each test.
    /// </summary>
    [global::NUnit.Framework.TearDown]
    public virtual void TearDown()
    {
        // Cleanup if needed
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
