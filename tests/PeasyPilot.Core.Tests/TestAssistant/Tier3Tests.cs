using PeasyPilot.TestAssistant.Generation;
using PeasyPilot.TestAssistant.Models;
using Xunit;

namespace PeasyPilot.Core.Tests.TestAssistant;

public class Tier3Tests
{
    [Fact]
    public void XUnitGenerator_GeneratesTestClass()
    {
        var plan = CreateTestPlan();
        var generator = new XUnitTestGenerator();

        var code = generator.GenerateTestClass(plan, "MyNamespace");

        Assert.NotNull(code);
        Assert.Contains("public class TestMethodTests", code);
        Assert.DoesNotContain("using NUnit.Framework;", code);
    }

    [Fact]
    public void XUnitGenerator_GeneratesFactAttribute()
    {
        var plan = CreateTestPlan();
        var generator = new XUnitTestGenerator();

        var code = generator.GenerateTestClass(plan, "MyNamespace");

        Assert.Contains("[Fact]", code);
    }

    [Fact]
    public void XUnitGenerator_GeneratesArrangeActAssert()
    {
        var scenario = new TestableScenario { Type = ScenarioType.HappyPath, Description = "test" };
        var model = new MethodTestModel { MethodName = "Add", TypeName = "Calculator", ReturnTypeName = "int" };
        var generator = new XUnitTestGenerator();

        var code = generator.GenerateTestMethod(scenario, model);

        Assert.Contains("// Arrange", code);
        Assert.Contains("// Act", code);
        Assert.Contains("// Assert", code);
    }

    [Fact]
    public void NUnitGenerator_GeneratesTestFixtureAttribute()
    {
        var plan = CreateTestPlan();
        var generator = new NUnitTestGenerator();

        var code = generator.GenerateTestClass(plan, "MyNamespace");

        Assert.Contains("[TestFixture]", code);
        Assert.Contains("[Test]", code);
    }

    [Fact]
    public void NUnitGenerator_GeneratesSetUpTearDown()
    {
        var plan = CreateTestPlan();
        var generator = new NUnitTestGenerator();

        var fixture = generator.GenerateTestFixture(plan);

        Assert.Contains("[SetUp]", fixture);
        Assert.Contains("[TearDown]", fixture);
    }

    [Fact]
    public void TUnitGenerator_GeneratesAsyncTask()
    {
        var plan = CreateTestPlan();
        var generator = new TUnitTestGenerator();

        var code = generator.GenerateTestClass(plan, "MyNamespace");

        Assert.Contains("public async Task", code);
    }

    [Fact]
    public void TestGeneratorRegistry_RegistersAllFrameworks()
    {
        var registry = new TestGeneratorRegistry();

        Assert.True(registry.IsSupported("xunit"));
        Assert.True(registry.IsSupported("nunit"));
        Assert.True(registry.IsSupported("tunit"));
    }

    [Fact]
    public void TestGeneratorRegistry_ReturnsCorrectGenerator()
    {
        var registry = new TestGeneratorRegistry();

        var xunitGen = registry.GetGenerator("xunit");
        var nunitGen = registry.GetGenerator("nunit");
        var tunitGen = registry.GetGenerator("tunit");

        Assert.NotNull(xunitGen);
        Assert.NotNull(nunitGen);
        Assert.NotNull(tunitGen);
        Assert.Equal("xunit", xunitGen.Framework);
        Assert.Equal("nunit", nunitGen.Framework);
        Assert.Equal("tunit", tunitGen.Framework);
    }

    [Fact]
    public void TestGeneratorRegistry_ThrowsOnUnknownFramework()
    {
        var registry = new TestGeneratorRegistry();

        Assert.Throws<InvalidOperationException>(() => registry.GetGenerator("unknown"));
    }

    [Fact]
    public void Generator_GeneratesMockSetup()
    {
        var dependencies = new List<DependencyInfo>
        {
            new DependencyInfo { InterfaceName = "ILogger", InjectionType = "Constructor" }
        };
        var generator = new XUnitTestGenerator();

        var mockCode = generator.GenerateMockSetup(dependencies);

        Assert.Contains("Mock", mockCode);
        Assert.Contains("ILogger", mockCode);
    }

    [Fact]
    public void Generator_ConvertsNamingConventions()
    {
        var generator = new XUnitTestGenerator();

        var pascalCase = generator.GetType().GetMethod("ToPascalCase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .Invoke(generator, new object[] { "test_method" });

        Assert.NotNull(pascalCase);
    }

    private TestPlan CreateTestPlan() =>
        new TestPlan
        {
            TypeName = "Calculator",
            MethodName = "TestMethod",
            Scenarios = new List<TestableScenario>
            {
                new TestableScenario { Type = ScenarioType.HappyPath, Description = "Happy path test" }
            },
            TotalEstimatedTests = 1,
            RiskScore = 3,
            EstimatedCoverage = 0.8m,
            RecommendedFramework = "xunit"
        };
}
