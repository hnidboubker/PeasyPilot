using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Composition;

public class TestBatteryComposer : ITestBatteryComposer
{
    public TestBatteryProposal Compose(
        Type targetType,
        ConstructorResolution constructorResolution,
        IReadOnlyList<ParameterValueCase> parameterCases,
        TestBatteryAnalysisOptions options)
    {
        var proposal = new TestBatteryProposal
        {
            TargetType = targetType.Name,
            TargetNamespace = targetType.Namespace ?? "Global",
            Framework = options.TargetFramework
        };

        var testCases = new List<TestCaseProposal>();

        var nominalCase = new TestCaseProposal
        {
            MethodName = targetType.Name,
            TestName = $"{targetType.Name}_CanInstantiate",
            Description = "Nominal case: can instantiate the target type",
            Category = "nominal",
            Source = "mechanical"
        };

        foreach (var param in constructorResolution.Parameters)
        {
            var expression = param.ResolutionKind switch
            {
                ParameterResolutionKind.Primitive => "default",
                ParameterResolutionKind.ConcreteNewable => $"new {param.Parameter.ParameterType.Name}()",
                ParameterResolutionKind.InterfaceOrAbstractNeedsMock =>
                    $"new MockFactory().Create(typeof({param.Parameter.ParameterType.Name}))",
                ParameterResolutionKind.Unresolvable =>
                    $"default // TODO: provide value for {param.Parameter.Name}",
                _ => "default"
            };

            nominalCase.ParameterValues[param.Parameter.Name ?? "unknown"] = new ParameterValue
            {
                Name = param.Parameter.Name ?? "unknown",
                Type = param.Parameter.ParameterType.Name,
                Expression = expression
            };
        }

        testCases.Add(nominalCase);

        foreach (var paramCase in parameterCases.Where(c => c.VariantName != "Sample"))
        {
            var variantCase = new TestCaseProposal
            {
                MethodName = targetType.Name,
                TestName = $"{targetType.Name}_With{paramCase.VariantName}",
                Description = paramCase.Description,
                Category = "boundary",
                Source = "mechanical"
            };

            foreach (var param in constructorResolution.Parameters)
            {
                variantCase.ParameterValues[param.Parameter.Name ?? "unknown"] = new ParameterValue
                {
                    Name = param.Parameter.Name ?? "unknown",
                    Type = param.Parameter.ParameterType.Name,
                    Expression = param.ResolutionKind == ParameterResolutionKind.Primitive ? paramCase.ValueExpression : "default"
                };
            }

            testCases.Add(variantCase);
        }

        proposal.TestCases.AddRange(testCases);
        return proposal;
    }
}
