namespace PeasyPilot.TestAssistant.Models;
public class MethodParameterInfo
{
    public required string Name { get; init; }
    public required string TypeName { get; init; }
    public Type? ResolvedType { get; init; }
    public bool IsNullable { get; init; }
    public bool IsValueType { get; init; }
    public bool IsCollection { get; init; }
    public bool IsInterface { get; init; }
    public Type? ResolvedInterfaceType { get; init; }
    public List<string> SuggestedTestValues { get; init; } = new();
}
