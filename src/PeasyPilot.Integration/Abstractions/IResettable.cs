namespace PeasyPilot.Integration.Abstractions;

/// <summary>
/// Interface for services that can be reset to their initial state.
/// Used for singleton services that maintain state between tests.
/// </summary>
public interface IResettable
{
    /// <summary>
    /// Resets the service to its initial state.
    /// </summary>
    Task ResetAsync();
}
