namespace PeasyPilot.XUnit;

using Xunit;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;

/// <summary>
/// Base class for xUnit test classes integrating with PeasyPilot.
/// </summary>
public abstract class PeasyPilotTestBase : IAsyncLifetime
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
    /// Initializes the test fixture asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task InitializeAsync()
    {
        TestContext = new TestContext();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the test fixture asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task DisposeAsync()
    {
        await Task.CompletedTask;
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
