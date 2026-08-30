namespace PeasyPilot.Core.Abstractions;

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

