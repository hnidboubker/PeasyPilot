namespace PeasyPilot.TestAssistant.Models;
public class MethodTestModel
{
    public required string TypeName { get; init; }
    public Type? ResolvedType { get; init; }
    public required string MethodName { get; init; }
    public bool IsAsync { get; init; }
    public bool IsStatic { get; init; }
    public bool IsPublic { get; init; }
    public required string ReturnTypeName { get; init; }
    public Type? ResolvedReturnType { get; init; }
    public List<MethodParameterInfo> Parameters { get; init; } = new();
    public List<DependencyInfo> Dependencies { get; init; } = new();
    public List<ExceptionInfo> PossibleExceptions { get; init; } = new();
    public List<TestableScenario> TestableScenarios { get; init; } = new();
    public int ComplexityScore { get; init; }
    public int EstimatedTestCases { get; init; }
    public string SuggestedFramework { get; init; } = "xunit";
}
