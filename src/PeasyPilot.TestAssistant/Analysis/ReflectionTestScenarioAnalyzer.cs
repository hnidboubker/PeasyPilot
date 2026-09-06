using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Rules;

namespace PeasyPilot.TestAssistant.Analysis;

public class ReflectionTestScenarioAnalyzer : ITestScenarioAnalyzer
{
    private readonly IConstructorResolutionStrategy _constructorStrategy;
    private readonly TypeShapeValueRuleRegistry _ruleRegistry;

    public ReflectionTestScenarioAnalyzer()
    {
        _constructorStrategy = new DefaultConstructorResolutionStrategy();
        _ruleRegistry = new TypeShapeValueRuleRegistry();
    }

    public TestBatteryProposal Analyze(Type targetType, TestBatteryAnalysisOptions options)
    {
        var resolution = _constructorStrategy.Resolve(targetType);
        var parameterCases = GenerateParameterCases(resolution, options);

        var proposal = new TestBatteryProposal
        {
            TargetType = targetType.Name,
            TargetNamespace = targetType.Namespace ?? "Global",
            Framework = options.TargetFramework
        };

        var methods = targetType
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .ToList();

        foreach (var method in methods)
        {
            var methodTestCases = GenerateTestCasesForMethod(method, resolution, options);
            proposal.TestCases.AddRange(methodTestCases);
        }

        return proposal;
    }

    private List<ParameterValueCase> GenerateParameterCases(ConstructorResolution resolution, TestBatteryAnalysisOptions options)
    {
        var context = new ValueGenerationContext(options.MaxEnumCases);
        var cases = new List<ParameterValueCase>();

        foreach (var param in resolution.Parameters)
        {
            var paramCases = _ruleRegistry.GenerateCases(param.Parameter.ParameterType, context);
            cases.AddRange(paramCases);
        }

        return cases;
    }

    private List<TestCaseProposal> GenerateTestCasesForMethod(System.Reflection.MethodInfo method, ConstructorResolution ctorResolution, TestBatteryAnalysisOptions options)
    {
        var cases = new List<TestCaseProposal>();
        var methodParams = method.GetParameters();
        var context = new ValueGenerationContext(options.MaxEnumCases);

        var nominalCase = new TestCaseProposal
        {
            MethodName = method.Name,
            TestName = $"{method.Name}_HappyPath",
            Description = $"Happy path test for {method.Name}",
            Category = "nominal",
            Source = "mechanical"
        };

        foreach (var param in methodParams)
        {
            nominalCase.ParameterValues[param.Name ?? "unknown"] = new ParameterValue
            {
                Name = param.Name ?? "unknown",
                Type = param.ParameterType.Name,
                Expression = "default",
                Variant = "nominal"
            };
        }

        cases.Add(nominalCase);

        return cases;
    }
}
