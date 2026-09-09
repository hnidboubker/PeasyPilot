using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Validation;
using Xunit;

namespace PeasyPilot.Core.Tests.TestAssistant;

public class Tier4Tests
{
    [Fact]
    public void ChallengeTest_IdentifiesMissingArrange()
    {
        var testCode = @"
            public void TestAdd()
            {
                // Act
                var result = Add(5, 3);
                // Assert
                Assert.Equal(8, result);
            }";
        var model = CreateMethodTestModel();
        var challenger = new TestChallenger();

        var result = challenger.ChallengeTest(testCode, model);

        Assert.Contains("Missing Arrange section", result.Issues);
    }

    [Fact]
    public void ChallengeTest_IdentifiesMissingActSection()
    {
        var testCode = @"
            public void TestAdd()
            {
                // Arrange
                var a = 5;
                var b = 3;
                // Assert
                Assert.Equal(8, a + b);
            }";
        var model = CreateMethodTestModel();
        var challenger = new TestChallenger();

        var result = challenger.ChallengeTest(testCode, model);

        Assert.Contains("Missing Act section", result.Issues);
    }

    [Fact]
    public void ChallengeTest_IdentifiesMissingAssertSection()
    {
        var testCode = @"
            public void TestAdd()
            {
                // Arrange
                var a = 5;
                var b = 3;
                // Act
                var result = Add(a, b);
            }";
        var model = CreateMethodTestModel();
        var challenger = new TestChallenger();

        var result = challenger.ChallengeTest(testCode, model);

        Assert.Contains("Missing Assert section", result.Issues);
    }

    [Fact]
    public void ChallengeTest_IdentifiesMissingAssertions()
    {
        var testCode = @"
            public void TestAdd()
            {
                // Arrange
                var a = 5;
                var b = 3;
                // Act
                var result = Add(a, b);
                // Assert
                // TODO: Add assertions
            }";
        var model = CreateMethodTestModel();
        var challenger = new TestChallenger();

        var result = challenger.ChallengeTest(testCode, model);

        Assert.Contains("No assertions found", result.Issues);
    }

    [Fact]
    public void ChallengeTest_ValidatesWellFormedTest()
    {
        var testCode = @"
            public void TestAdd_WhenAddingNumbers_ShouldReturnSum()
            {
                // Arrange
                var a = 5;
                var b = 3;
                // Act
                var result = Add(a, b);
                // Assert
                Assert.Equal(8, result);
            }";
        var model = CreateMethodTestModel();
        var challenger = new TestChallenger();

        var result = challenger.ChallengeTest(testCode, model);

        Assert.Empty(result.Issues);
        Assert.True(result.PassesBasicValidation);
    }

    [Fact]
    public void ChallengeTest_DetectsAsyncIssues()
    {
        var testCode = @"
            public void TestProcessAsync()
            {
                // Arrange
                var data = new Data();
                // Act
                var result = ProcessAsync(data);
                // Assert
                Assert.NotNull(result);
            }";
        var model = new MethodTestModel
        {
            MethodName = "ProcessAsync",
            TypeName = "Service",
            ReturnTypeName = "Task",
            IsAsync = true
        };
        var challenger = new TestChallenger();

        var result = challenger.ChallengeTest(testCode, model);

        Assert.Contains("Async method test missing async/await", result.Issues);
    }

    [Fact]
    public void ChallengeTestSuite_GeneratesReport()
    {
        var testCodes = new List<string>
        {
            @"public void TestAdd() { var r = Add(5, 3); Assert.Equal(8, r); }",
            @"public void TestSubtract() { var r = Subtract(5, 3); Assert.Equal(2, r); }"
        };
        var model = CreateMethodTestModel();
        var challenger = new TestChallenger();

        var report = challenger.ChallengeTestSuite(testCodes, model);

        Assert.Equal(2, report.TotalTestsChallenged);
        Assert.NotEmpty(report.OverallRecommendation);
    }

    [Fact]
    public void ScoreTestQuality_ScoresWellFormedTest()
    {
        var testCode = @"
            public void TestAdd_WhenAddingNumbers_ShouldReturnSum()
            {
                // Arrange
                var a = 5;
                // Act
                var result = Add(a, 3);
                // Assert
                Assert.Equal(8, result);
            }";
        var scenario = new TestableScenario { Type = ScenarioType.HappyPath, Description = "test" };
        var challenger = new TestChallenger();

        var score = challenger.ScoreTestQuality(testCode, scenario);

        Assert.True(score > 50);
    }

    [Fact]
    public void ValidateCoverage_IdentifiesGaps()
    {
        var plan = new TestPlan
        {
            MethodName = "Calculate",
            TypeName = "Calculator",
            Scenarios = new List<TestableScenario>
            {
                new TestableScenario { Type = ScenarioType.HappyPath, Description = "normal case" },
                new TestableScenario { Type = ScenarioType.Error, Description = "error case" },
                new TestableScenario { Type = ScenarioType.Boundary, Description = "boundary case" }
            }
        };
        var coveredScenarios = new List<TestableScenario>
        {
            new TestableScenario { Type = ScenarioType.HappyPath, Description = "normal case" }
        };
        var challenger = new TestChallenger();

        var gaps = challenger.ValidateCoverage(plan, coveredScenarios);

        Assert.Contains(gaps, g => g.Contains("Error"));
        Assert.Contains(gaps, g => g.Contains("Boundary"));
    }

    [Fact]
    public void ChallengeTest_SuggestsMockingForDependencies()
    {
        var testCode = @"
            public void TestServiceCall()
            {
                // Arrange
                var service = new RealService();
                // Act
                var result = service.Execute();
                // Assert
                Assert.NotNull(result);
            }";
        var model = new MethodTestModel
        {
            MethodName = "Execute",
            TypeName = "Service",
            ReturnTypeName = "Result",
            Dependencies = new List<DependencyInfo>
            {
                new DependencyInfo { InterfaceName = "IRepository", InjectionType = "Constructor" }
            }
        };
        var challenger = new TestChallenger();

        var result = challenger.ChallengeTest(testCode, model);

        Assert.Contains(result.Suggestions, s => s.Contains("mock"));
    }

    [Fact]
    public void ChallengeTestSuite_CalculatesAverageScore()
    {
        var testCodes = new List<string>
        {
            @"public void Test1() { Assert.True(true); }",
            @"public void Test2() { Assert.True(true); }",
            @"public void Test3() { Assert.True(true); }"
        };
        var model = CreateMethodTestModel();
        var challenger = new TestChallenger();

        var report = challenger.ChallengeTestSuite(testCodes, model);

        Assert.True(report.AverageQualityScore >= 0);
        Assert.True(report.AverageQualityScore <= 100);
    }

    private MethodTestModel CreateMethodTestModel() =>
        new MethodTestModel
        {
            TypeName = "Calculator",
            MethodName = "Add",
            ReturnTypeName = "int",
            IsAsync = false,
            IsPublic = true,
            Parameters = new List<MethodParameterInfo>
            {
                new MethodParameterInfo { Name = "a", TypeName = "int" },
                new MethodParameterInfo { Name = "b", TypeName = "int" }
            }
        };
}
