namespace PeasyPilot.Core.Abstractions;

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

