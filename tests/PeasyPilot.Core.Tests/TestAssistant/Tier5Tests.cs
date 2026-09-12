using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Orchestration;
using Xunit;
using PeasyPilot.XUnit;

namespace PeasyPilot.Core.Tests.TestAssistant;

/// <summary>
/// Test calculator class for AI Test Engineer tests.
/// </summary>
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}

public class Tier5Tests
{
    [Fact]
    public void AITestEngineer_AnalyzesMethod()
    {
        var engine = new AITestEngineerBuilder()
            .WithAnalyzer(new DummyAnalyzer())
            .Build();

        var analysis = engine.AnalyzeMethod("Calculator", "Add");

        XAssert.NotNull(analysis);
        XAssert.Equal("Calculator", analysis.TypeName);
    }

    [Fact]
    public void AITestEngineer_PlansTests()
    {
        var engine = AITestEngineerBuilder.CreateDefault().Build();
        var analysis = CreateTestModel();

        var plan = engine.PlanTests(analysis);

        XAssert.NotNull(plan);
        XAssert.True(plan.TotalEstimatedTests > 0);
    }

    [Fact]
    public void AITestEngineer_GeneratesTestCode()
    {
        var engine = AITestEngineerBuilder.CreateDefault().Build();
        var plan = CreateTestPlan();

        var code = engine.GenerateTests(plan, "TestNamespace", "xunit");

        XAssert.NotNull(code);
        XAssert.NotEmpty(code);
        // Todo Corrige cette partie 
        XAssert.Contains("public class", code);
    }

    [Fact]
    public void AITestEngineer_ChallengesGeneratedCode()
    {
        var engine = AITestEngineerBuilder.CreateDefault().Build();
        var testCode = @"
            public void TestAdd()
            {
                // Arrange
                var a = 5;
                var b = 3;
                // Act
                var result = Add(a, b);
                // Assert
                Assert.Equal(8, result);
            }";
        var model = CreateTestModel();

        var result = engine.ChallengeTests(testCode, model);

        XAssert.NotNull(result);
        XAssert.True(result.PassesBasicValidation);
    }
    public void TestAdd()
    {
        // Arrange
        var a = 5;
        var b = 3;
        // Act
        var calcalute = new Calculator();
        var result = calcalute.Add(a, b);
        // Assert
        XAssert.Equal(8, result);
    }
    [Fact]
    public void AITestEngineer_ExecutesCompleteWorkflow()
    {
        var engine = new AITestEngineerBuilder()
            .WithAnalyzer(new DummyAnalyzer())
            .Build();

        var result = engine.ExecuteCompleteWorkflow("Calculator", "Add", "CalcTests", "xunit");

        XAssert.NotNull(result);
        XAssert.Equal("Add", result.MethodName);
        XAssert.NotNull(result.Analysis);
        XAssert.NotNull(result.Plan);
        XAssert.NotNull(result.GeneratedCode);
        XAssert.NotNull(result.Challenge);
        XAssert.NotEmpty(result.NextSteps);
    }

    [Fact]
    public void AITestEngineer_WorkflowProducesQualityScore()
    {
        var engine = new AITestEngineerBuilder()
            .WithAnalyzer(new DummyAnalyzer())
            .Build();

        var result = engine.ExecuteCompleteWorkflow("Calculator", "Add", "CalcTests", "xunit");

        XAssert.True(result.TotalQualityScore >= 0);
        XAssert.True(result.TotalQualityScore <= 100);
    }

    [Fact]
    public void AITestEngineer_WorkflowDeterminesProduction()
    {
        var engine = new AITestEngineerBuilder()
            .WithAnalyzer(new DummyAnalyzer())
            .Build();

        var result = engine.ExecuteCompleteWorkflow("Calculator", "Add", "CalcTests", "xunit");

        XAssert.NotNull(result);
        // Production recommendation depends on quality
        XAssert.IsType<bool>(result.IsRecommendedForProduction);
    }

    [Fact]
    public void AITestEngineerBuilder_SupportsFluentConfiguration()
    {
        var engine = AITestEngineerBuilder.CreateDefault()
            .WithAnalyzer(new DummyAnalyzer())
            .Build();

        XAssert.NotNull(engine);
        XAssert.IsAssignableFrom<IAITestEngineer>(engine);
    }

    [Fact]
    public void AITestEngineerBuilder_UsesDefaultsWhenNotProvided()
    {
        var engine = new AITestEngineerBuilder().UseDefaults().Build();

        XAssert.NotNull(engine);
    }

    [Fact]
    public void AITestEngineer_SupportsMultipleFrameworks()
    {
        var engine = AITestEngineerBuilder.CreateDefault().Build();
        var plan = CreateTestPlan();

        var xunitCode = engine.GenerateTests(plan, "Tests", "xunit");
        var nunitCode = engine.GenerateTests(plan, "Tests", "nunit");
        var tunitCode = engine.GenerateTests(plan, "Tests", "tunit");

        XAssert.NotEmpty(xunitCode);
        XAssert.NotEmpty(nunitCode);
        XAssert.NotEmpty(tunitCode);
    }

    [Fact]
    public void AITestEngineer_GeneratesAppropriateNextSteps()
    {
        var engine = new AITestEngineerBuilder()
            .WithAnalyzer(new DummyAnalyzer())
            .Build();

        var result = engine.ExecuteCompleteWorkflow("Calculator", "Add", "CalcTests", "xunit");

        XAssert.NotEmpty(result.NextSteps);
        // Should contain actionable guidance
        var hasGuidance = result.NextSteps.Any(s =>
            s.Contains("Fix") || s.Contains("Consider") || s.Contains("Add") || s.Contains("production"));
        XAssert.True(hasGuidance);
    }

    private class DummyAnalyzer : ICodeAnalyzer
    {
        public Task<IReadOnlyList<MethodTestModel>> AnalyzeFileAsync(string filePath)
            => Task.FromResult<IReadOnlyList<MethodTestModel>>(new[] { CreateTestModel() });

        public Task<IReadOnlyList<MethodTestModel>> AnalyzeTypeAsync(Type type)
            => Task.FromResult<IReadOnlyList<MethodTestModel>>(new[] { CreateTestModel() });

        public Task<MethodTestModel?> AnalyzeMethodAsync(Type? type, string methodName)
            => Task.FromResult<MethodTestModel?>(CreateTestModel());
    }

    private static MethodTestModel CreateTestModel() =>
        new MethodTestModel
        {
            TypeName = "Calculator",
            MethodName = "Add",
            ReturnTypeName = "int",
            Parameters = new List<MethodParameterInfo>
            {
                new MethodParameterInfo { Name = "a", TypeName = "int" },
                new MethodParameterInfo { Name = "b", TypeName = "int" }
            },
            TestableScenarios = new List<TestableScenario>
            {
                new TestableScenario { Type = ScenarioType.HappyPath, Description = "Add positive numbers" }
            }
        };

    private static TestPlan CreateTestPlan() =>
        new TestPlan
        {
            MethodName = "Add",
            TypeName = "Calculator",
            Scenarios = new List<TestableScenario>
            {
                new TestableScenario { Type = ScenarioType.HappyPath, Description = "Add positive numbers" }
            },
            TotalEstimatedTests = 3,
            RiskScore = 2
        };
}
