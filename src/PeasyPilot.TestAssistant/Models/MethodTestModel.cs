namespace PeasyPilot.TestAssistant.Models;

/// <summary>
/// Complete metadata for a method that can be tested.
/// </summary>
public class MethodTestModel
{
    /// <summary>
    /// Type containing the method
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Resolved CLR Type
    /// </summary>
    public Type? ResolvedType { get; init; }

    /// <summary>
    /// Method name
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// Whether the method is async (returns Task or Task<T>)
    /// </summary>
    public bool IsAsync { get; init; }

    /// <summary>
    /// Whether the method is static
    /// </summary>
    public bool IsStatic { get; init; }

    /// <summary>
    /// Whether the method is public
    /// </summary>
    public bool IsPublic { get; init; }

    /// <summary>
    /// Return type name (void, int, Task<User>, etc.)
    /// </summary>
    public required string ReturnTypeName { get; init; }

    /// <summary>
    /// Resolved CLR return type
    /// </summary>
    public Type? ResolvedReturnType { get; init; }

    /// <summary>
    /// Method parameters
    /// </summary>
    public List<MethodParameterInfo> Parameters { get; init; } = new();

    /// <summary>
    /// Dependencies injected through constructor
    /// </summary>
    public List<DependencyInfo> Dependencies { get; init; } = new();

    /// <summary>
    /// Exceptions the method might throw
    /// </summary>
    public List<ExceptionInfo> PossibleExceptions { get; init; } = new();

    /// <summary>
    /// Testable scenarios for this method
    /// </summary>
    public List<TestableScenario> TestableScenarios { get; init; } = new();

    /// <summary>
    /// Overall test complexity score (1-10)
    /// </summary>
    public int ComplexityScore { get; init; }

    /// <summary>
    /// Estimated number of test cases needed for 80% coverage
    /// </summary>
    public int EstimatedTestCases { get; init; }

    /// <summary>
    /// Suggested test framework (xunit, nunit, tunit)
    /// </summary>
    public string SuggestedFramework { get; init; } = "xunit";
}
