namespace global::PeasyPilot.Core.Abstractions;

/// <summary>
/// Provides storage for test context data.
/// </summary>
public interface ITestContext
{
    /// <summary>
    /// Gets or adds a value to the test context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if it doesn't exist.</param>
    /// <returns>The value from the cache or newly created value.</returns>
    T GetOrAdd<T>(string key, Func<T> factory);
}

