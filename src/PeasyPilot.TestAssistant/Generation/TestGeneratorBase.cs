using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
using System.Text;

namespace PeasyPilot.TestAssistant.Generation;

public abstract class TestGeneratorBase : ITestGenerator
{
    public abstract string Framework { get; }

    public abstract string GenerateTestClass(TestPlan plan, string @namespace);

    public abstract string GenerateTestMethod(TestableScenario scenario, MethodTestModel model);

    public abstract string GenerateTestFixture(TestPlan plan);

    public abstract string GenerateMockSetup(List<DependencyInfo> dependencies);

    protected string GenerateUsings(TestPlan plan)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Threading.Tasks;");

        if (plan.RequiresMocking)
            sb.AppendLine("using Moq;");

        if (plan.RequiresIntegration)
            sb.AppendLine("using PeasyPilot.Integration;");

        return sb.ToString();
    }

    protected string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    protected string ToSnakeCase(string input)
    {
        var result = new StringBuilder();
        foreach (var c in input)
        {
            if (char.IsUpper(c) && result.Length > 0)
                result.Append('_');
            result.Append(char.ToLower(c));
        }
        return result.ToString();
    }

    protected string GenerateArrangeSection(MethodTestModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            // Arrange");

        foreach (var param in model.Parameters)
            sb.AppendLine($"            var {ToSnakeCase(param.Name)} = default({param.TypeName});");

        return sb.ToString();
    }

    protected string GenerateActSection(MethodTestModel model)
    {
        var methodCall = $"{model.MethodName}({string.Join(", ", model.Parameters.Select(p => ToSnakeCase(p.Name)))})";
        var sb = new StringBuilder();

        sb.AppendLine("            // Act");
        if (model.IsAsync)
            sb.AppendLine($"            var result = await {methodCall};");
        else
            sb.AppendLine($"            var result = {methodCall};");

        return sb.ToString();
    }

    protected string GenerateAssertSection(TestableScenario scenario)
    {
        var sb = new StringBuilder();
        sb.AppendLine("            // Assert");
        sb.AppendLine($"            Assert.NotNull(result); // TODO: Add specific assertions for {scenario.Description}");

        return sb.ToString();
    }

    protected string GenerateScenarioName(TestableScenario scenario)
    {
        var baseName = scenario.SuggestedTestName ?? $"Test{scenario.Type}";
        return ToPascalCase(baseName);
    }
}
