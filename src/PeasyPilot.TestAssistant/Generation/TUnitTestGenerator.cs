using PeasyPilot.TestAssistant.Models;
using System.Text;

namespace PeasyPilot.TestAssistant.Generation;

public class TUnitTestGenerator : TestGeneratorBase
{
    public override string Framework => "tunit";

    public override string GenerateTestClass(TestPlan plan, string @namespace)
    {
        var sb = new StringBuilder();

        sb.AppendLine(GenerateUsings(plan));
        sb.AppendLine();
        sb.AppendLine($"namespace {@namespace}.Tests;");
        sb.AppendLine();
        sb.AppendLine($"public class {ToPascalCase(plan.MethodName)}Tests");
        sb.AppendLine("{");
        sb.AppendLine(GenerateTestFixture(plan));
        sb.AppendLine();

        foreach (var scenario in plan.Scenarios)
        {
            sb.AppendLine(GenerateTestMethod(scenario, new MethodTestModel
            {
                MethodName = plan.MethodName,
                TypeName = plan.TypeName,
                ReturnTypeName = "void",
                IsAsync = true,
                Parameters = new(),
                Dependencies = new()
            }));
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    public override string GenerateTestMethod(TestableScenario scenario, MethodTestModel model)
    {
        var sb = new StringBuilder();
        var testName = GenerateScenarioName(scenario);
        var signature = model.IsAsync ? "public async Task" : "public void";

        sb.AppendLine($"    {signature} {testName}()");
        sb.AppendLine("    {");
        sb.AppendLine(GenerateArrangeSection(model));
        sb.AppendLine();
        sb.AppendLine(GenerateActSection(model));
        sb.AppendLine();
        sb.AppendLine(GenerateAssertSection(scenario));
        sb.AppendLine("    }");

        return sb.ToString();
    }

    public override string GenerateTestFixture(TestPlan plan)
    {
        var sb = new StringBuilder();
        sb.AppendLine("    private readonly IServiceProvider _serviceProvider;");
        sb.AppendLine();
        sb.AppendLine($"    public {ToPascalCase(plan.MethodName)}Tests()");
        sb.AppendLine("    {");
        sb.AppendLine("        _serviceProvider = new ServiceCollection()");
        sb.AppendLine("            .BuildServiceProvider();");
        sb.AppendLine("    }");

        return sb.ToString();
    }

    public override string GenerateMockSetup(List<DependencyInfo> dependencies)
    {
        var sb = new StringBuilder();

        foreach (var dep in dependencies.Where(d => d.ShouldMock))
        {
            var varName = ToSnakeCase(dep.InterfaceName.TrimStart('I'));
            sb.AppendLine($"        var {varName}Mock = new Mock<{dep.InterfaceName}>();");
        }

        return sb.ToString();
    }
}
