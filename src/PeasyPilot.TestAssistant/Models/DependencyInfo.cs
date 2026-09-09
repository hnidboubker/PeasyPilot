namespace PeasyPilot.TestAssistant.Models;
public class DependencyInfo
{
    public required string InterfaceName { get; init; }
    public Type? ResolvedType { get; init; }
    public required string InjectionType { get; init; }
    public bool IsRequired { get; init; }
    public string SuggestedStrategy { get; init; } = "Moq.Mock";
    public bool ShouldMock => SuggestedStrategy.Contains("Moq");
    public bool ShouldFixture => SuggestedStrategy.Contains("Fixture");
}
