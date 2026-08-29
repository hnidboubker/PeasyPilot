namespace PeasyPilot.Core.Abstractions;

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

/// <summary>
/// Factory for creating test data.
/// </summary>
public interface ITestDataFactory
{
    /// <summary>
    /// Creates a single instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    /// <returns>A new instance of type T.</returns>
    T Create<T>() where T : class;

    /// <summary>
    /// Creates multiple instances of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    /// <param name="count">The number of instances to create.</param>
    /// <returns>A collection of instances.</returns>
    IReadOnlyCollection<T> CreateMany<T>(int count) where T : class;
}

/// <summary>
/// Factory for creating mock objects.
/// </summary>
public interface IMockFactory
{
    /// <summary>
    /// Creates a mock instance of the specified type.
    /// </summary>
    /// <param name="type">The type to mock.</param>
    /// <returns>A mock object.</returns>
    object Create(Type type);
}

/// <summary>
/// Represents the test environment.
/// </summary>
public interface ITestEnvironment
{
}

