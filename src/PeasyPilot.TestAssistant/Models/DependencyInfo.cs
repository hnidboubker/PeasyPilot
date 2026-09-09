namespace PeasyPilot.TestAssistant.Models;

/// <summary>
/// Represents a dependency detected in a type or method.
/// </summary>
public class DependencyInfo
{
    /// <summary>
    /// Dependency interface name (fully qualified)
    /// </summary>
    public required string InterfaceName { get; init; }

    /// <summary>
    /// CLR Type if resolved
    /// </summary>
    public Type? ResolvedType { get; init; }

    /// <summary>
    /// How the dependency is injected (Constructor, Property, Method Parameter, etc.)
    /// </summary>
    public required string InjectionType { get; init; }

    /// <summary>
    /// Whether the dependency is required (non-nullable)
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Suggested mock/fixture strategy for this dependency
    /// </summary>
    public string SuggestedStrategy { get; init; } = "Moq.Mock";

    /// <summary>
    /// Whether the dependency should be mocked in unit tests
    /// </summary>
    public bool ShouldMock => SuggestedStrategy.Contains("Moq");

    /// <summary>
    /// Whether the dependency should use a fixture/integration test
    /// </summary>
    public bool ShouldFixture => SuggestedStrategy.Contains("Fixture");
}
