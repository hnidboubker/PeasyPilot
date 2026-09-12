using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Planning;
using PeasyPilot.XUnit;

using Xunit;

namespace PeasyPilot.Core.Tests.TestAssistant;

public class Tier2Tests
{
    [Fact]
    public void BuildPlan_ReturnsValidTestPlan()
    {
        var model = CreateMethodTestModel();
        var builder = new TestPlanBuilder();

        var plan = builder.BuildPlan(model);

        XAssert.NotNull(plan);
        XAssert.Equal("TestClass", plan.TypeName);
        XAssert.Equal("TestMethod", plan.MethodName);
        XAssert.True(plan.TotalEstimatedTests > 0);
    }

    [Fact]
    public void BuildPlan_CorrectlyEstimatesTestCount()
    {
        var model = new MethodTestModel
        {
            TypeName = "Calculator",
            MethodName = "Add",
            ReturnTypeName = "int",
            Parameters = new List<MethodParameterInfo>
            {
                new MethodParameterInfo { Name = "a", TypeName = "int", IsNullable = false },
                new MethodParameterInfo { Name = "b", TypeName = "int", IsNullable = false }
            },
            TestableScenarios = new List<TestableScenario>
            {
                new TestableScenario { Type = ScenarioType.HappyPath, Description = "Add positive numbers" },
                new TestableScenario { Type = ScenarioType.Boundary, Description = "Add zero" }
            }
        };
        var builder = new TestPlanBuilder();

        var plan = builder.BuildPlan(model);

        XAssert.True(plan.TotalEstimatedTests >= 2);
    }

    [Fact]
    public void BuildPlan_CorrectlyScoresRisk()
    {
        var model = new MethodTestModel
        {
            TypeName = "AsyncService",
            MethodName = "ProcessAsync",
            ReturnTypeName = "Task",
            IsAsync = true,
            Dependencies = new List<DependencyInfo>
            {
                new DependencyInfo { InterfaceName = "IRepository", IsRequired = true, InjectionType = "Constructor" }
            },
            PossibleExceptions = new List<ExceptionInfo>
            {
                new ExceptionInfo { ExceptionTypeName = "InvalidOperationException" }
            }
        };
        var builder = new TestPlanBuilder();

        var plan = builder.BuildPlan(model);

        XAssert.True(plan.RiskScore > 0);
        XAssert.True(plan.RiskScore <= 10);
    }

    [Fact]
    public void BuildPlan_IdentifiesCoverageGaps()
    {
        var model = new MethodTestModel
        {
            TypeName = "Service",
            MethodName = "Execute",
            ReturnTypeName = "void",
            TestableScenarios = new List<TestableScenario>()
        };
        var builder = new TestPlanBuilder();

        var plan = builder.BuildPlan(model);

        XAssert.NotEmpty(plan.CoverageGaps);
        XAssert.Contains("Missing happy path scenario", plan.CoverageGaps);
    }

    [Fact]
    public void BuildPlan_DetectsIntegrationNeeds()
    {
        var model = new MethodTestModel
        {
            TypeName = "UserService",
            MethodName = "GetUser",
            ReturnTypeName = "User",
            Dependencies = new List<DependencyInfo>
            {
                new DependencyInfo { InterfaceName = "IUserRepository", IsRequired = true, InjectionType = "Constructor" }
            }
        };
        var builder = new TestPlanBuilder();

        var plan = builder.BuildPlan(model);

        XAssert.True(plan.RequiresIntegration);
    }

    [Fact]
    public void BuildPlan_DetectsMockingNeeds()
    {
        var model = new MethodTestModel
        {
            TypeName = "OrderService",
            MethodName = "CreateOrder",
            ReturnTypeName = "Order",
            Dependencies = new List<DependencyInfo>
            {
                new DependencyInfo { InterfaceName = "ILogger", InjectionType = "Constructor" }
            }
        };
        var builder = new TestPlanBuilder();

        var plan = builder.BuildPlan(model);

        XAssert.True(plan.RequiresMocking);
    }

    [Fact]
    public void EnrichPlan_AddsIntegrationPatterns()
    {
        var plan = new TestPlan
        {
            TypeName = "Service",
            MethodName = "Execute",
            RecommendedPatterns = new List<string>()
        };
        var dependencies = new List<DependencyInfo>
        {
            new DependencyInfo { InterfaceName = "IRepository", IsRequired = true, InjectionType = "Constructor" }
        };
        var builder = new TestPlanBuilder();

        var enriched = builder.EnrichPlan(plan, dependencies);

        XAssert.True(enriched.RequiresIntegration);
    }

    [Fact]
    public void EnrichPlan_AddsMockingPatterns()
    {
        var plan = new TestPlan
        {
            TypeName = "Service",
            MethodName = "Execute",
            RecommendedPatterns = new List<string>()
        };
        var dependencies = new List<DependencyInfo>
        {
            new DependencyInfo { InterfaceName = "ILogger", InjectionType = "Constructor" }
        };
        var builder = new TestPlanBuilder();

        var enriched = builder.EnrichPlan(plan, dependencies);

        XAssert.True(enriched.RequiresMocking);
    }

    [Fact]
    public void TestQualityScorer_CalculatesCoverageCorrectly()
    {
        var model = new MethodTestModel
        {
            TypeName = "Test",
            MethodName = "Method",
            ReturnTypeName = "void",
            TestableScenarios = new List<TestableScenario>
            {
                new TestableScenario { Type = ScenarioType.HappyPath, Description = "test" }
            }
        };
        var scorer = new TestQualityScorer();

        var coverage = scorer.CalculateCoverage(model);

        XAssert.True(coverage > 0);
        XAssert.True(coverage <= 1);
    }

    [Fact]
    public void TestQualityScorer_EstimatesTestCountAccurately()
    {
        var model = new MethodTestModel
        {
            TypeName = "Calculator",
            MethodName = "Divide",
            ReturnTypeName = "decimal",
            Parameters = new List<MethodParameterInfo>
            {
                new MethodParameterInfo { Name = "numerator", TypeName = "decimal" },
                new MethodParameterInfo { Name = "denominator", TypeName = "decimal" }
            },
            PossibleExceptions = new List<ExceptionInfo>
            {
                new ExceptionInfo { ExceptionTypeName = "DivideByZeroException" }
            }
        };
        var scorer = new TestQualityScorer();

        var count = scorer.EstimateTestCount(model);

        XAssert.True(count >= 2);
    }

    private MethodTestModel CreateMethodTestModel() =>
        new MethodTestModel
        {
            TypeName = "TestClass",
            MethodName = "TestMethod",
            ReturnTypeName = "void",
            IsAsync = false,
            IsPublic = true,
            ComplexityScore = 3
        };
}
