namespace PeasyPilot.TestAssistant.Models;

/// <summary>
/// Represents metadata about a method parameter (Tier 1 - Code Analysis).
/// </summary>
public class MethodParameterInfo
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Parameter type (fully qualified)
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// CLR Type if resolved
    /// </summary>
    public Type? ResolvedType { get; init; }

    /// <summary>
    /// Whether the parameter is nullable
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Whether the parameter is a value type
    /// </summary>
    public bool IsValueType { get; init; }

    /// <summary>
    /// Whether the parameter is a collection
    /// </summary>
    public bool IsCollection { get; init; }

    /// <summary>
    /// Whether the parameter is an interface (likely a dependency)
    /// </summary>
    public bool IsInterface { get; init; }

    /// <summary>
    /// Optional: Resolved interface type if this is an interface parameter
    /// </summary>
    public Type? ResolvedInterfaceType { get; init; }

    /// <summary>
    /// Suggested test values for this parameter
    /// </summary>
    public List<string> SuggestedTestValues { get; init; } = new();
}
