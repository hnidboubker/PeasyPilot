namespace PeasyPilot.TestAssistant.Models;
public class ExceptionInfo
{
    public required string ExceptionTypeName { get; init; }
    public Type? ResolvedType { get; init; }
    public string? Description { get; init; }
    public string? SuggestedTestScenario { get; init; }
}
