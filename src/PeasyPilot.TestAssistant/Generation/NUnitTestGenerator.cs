using PeasyPilot.TestAssistant.Models;
using System.Text;

namespace PeasyPilot.TestAssistant.Generation;

public class NUnitTestGenerator : TestGeneratorBase
{
    public override string Framework => "nunit";

    public override string GenerateTestClass(TestPlan plan, string @namespace)
    {
        var sb = new StringBuilder();

        sb.AppendLine(GenerateUsings(plan));
        sb.AppendLine("using NUnit.Framework;");
        sb.AppendLine();
        sb.AppendLine($"namespace {@namespace}.Tests;");
        sb.AppendLine();
        sb.AppendLine("[TestFixture]");
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

        sb.AppendLine($"    [Test]");
        sb.AppendLine($"    public void {testName}()");
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
        sb.AppendLine("    private IServiceProvider _serviceProvider;");
        sb.AppendLine();
        sb.AppendLine("    [SetUp]");
        sb.AppendLine("    public void SetUp()");
        sb.AppendLine("    {");
        sb.AppendLine("        _serviceProvider = new ServiceCollection()");
        sb.AppendLine("            .BuildServiceProvider();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [TearDown]");
        sb.AppendLine("    public void TearDown()");
        sb.AppendLine("    {");
        sb.AppendLine("        _serviceProvider?.Dispose();");
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
