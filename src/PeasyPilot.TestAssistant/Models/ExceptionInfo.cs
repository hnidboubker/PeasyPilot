namespace PeasyPilot.TestAssistant.Models;

/// <summary>
/// Represents an exception that a method might throw.
/// </summary>
public class ExceptionInfo
{
    /// <summary>
    /// Exception type name (fully qualified)
    /// </summary>
    public required string ExceptionTypeName { get; init; }

    /// <summary>
    /// CLR Type if resolved
    /// </summary>
    public Type? ResolvedType { get; init; }

    /// <summary>
    /// Description of when this exception is thrown
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Suggested test case to trigger this exception
    /// </summary>
    public string? SuggestedTestScenario { get; init; }
}
