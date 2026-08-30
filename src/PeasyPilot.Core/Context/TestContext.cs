using PeasyPilot.Core.Abstractions;
namespace PeasyPilot.Core.Context;

using System.Collections.Concurrent;
/// <summary>
/// Thread-safe context for storing test data.
/// </summary>
public class TestContext : ITestContext
{
    private readonly ConcurrentDictionary<string, object> _data = new();

    /// <summary>
    /// Gets or adds a value to the test context.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if it doesn't exist.</param>
    /// <returns>The value from the cache or newly created value.</returns>
    public T GetOrAdd<T>(string key, Func<T> factory)
    {
        return (T)_data.GetOrAdd(key, _ => factory()!);
    }
}
