using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
using System.Text.RegularExpressions;

namespace PeasyPilot.TestAssistant.Validation;

public class TestChallenger : ITestChallenger
{
    public TestChallengeResult ChallengeTest(string testCode, MethodTestModel model)
    {
        var issues = new List<string>();
        var suggestions = new List<string>();
        var qualityScore = 100;

        // Check for Arrange-Act-Assert pattern
        if (!testCode.Contains("// Arrange"))
            issues.Add("Missing Arrange section");
        if (!testCode.Contains("// Act"))
            issues.Add("Missing Act section");
        if (!testCode.Contains("// Assert"))
            issues.Add("Missing Assert section");

        // Check for assertions
        if (!ContainsAssertions(testCode))
        {
            issues.Add("No assertions found");
            qualityScore -= 30;
        }

        // Check for proper naming
        if (!HasGoodTestName(testCode))
            suggestions.Add("Use descriptive test names (TestWhen_Should_...)");

        // Check for mocking if dependencies exist
        if (model.Dependencies.Any() && !testCode.Contains("Mock"))
            suggestions.Add("Consider mocking dependencies");

        // Check for async/await if method is async
        if (model.IsAsync && !testCode.Contains("async") && !testCode.Contains("await"))
            issues.Add("Async method test missing async/await");

        // Reduce score based on issues
        qualityScore -= issues.Count * 15;
        qualityScore = Math.Max(0, qualityScore);

        return new TestChallengeResult
        {
            TestName = ExtractTestName(testCode),
            QualityScore = qualityScore,
            Issues = issues,
            Suggestions = suggestions,
            PassesBasicValidation = issues.Count == 0,
            EstimatedCoverage = CalculateCoverage(testCode),
            ComplexityScore = CalculateComplexity(testCode),
            MissingScenarios = new()
        };
    }

    public TestChallengeReport ChallengeTestSuite(List<string> testCodes, MethodTestModel model)
    {
        var results = testCodes.Select(code => ChallengeTest(code, model)).ToList();
        var passedCount = results.Count(r => r.PassesBasicValidation);
        var avgScore = results.Any() ? (decimal)results.Average(r => r.QualityScore) : 0m;

        var criticalIssues = results
            .SelectMany(r => r.Issues)
            .Distinct()
            .ToList();

        var recommendation = GenerateRecommendation(passedCount, results.Count, avgScore, criticalIssues);

        return new TestChallengeReport
        {
            MethodName = model.MethodName,
            TotalTestsChallenged = testCodes.Count,
            PassedCount = passedCount,
            AverageQualityScore = avgScore,
            Results = results,
            CriticalIssues = criticalIssues,
            OverallRecommendation = recommendation
        };
    }

    public List<string> ValidateCoverage(TestPlan plan, List<TestableScenario> coveredScenarios)
    {
        var gaps = new List<string>();
        var planScenarios = plan.Scenarios.Select(s => s.Type).Distinct();
        var covered = coveredScenarios.Select(s => s.Type).Distinct();

        foreach (var scenarioType in planScenarios)
        {
            if (!covered.Contains(scenarioType))
                gaps.Add($"Missing coverage for {scenarioType} scenarios");
        }

        return gaps;
    }

    public int ScoreTestQuality(string testCode, TestableScenario scenario)
    {
        int score = 50;

        if (ContainsAssertions(testCode))
            score += 20;

        if (testCode.Contains("// Arrange") && testCode.Contains("// Act") && testCode.Contains("// Assert"))
            score += 15;

        if (HasGoodTestName(testCode))
            score += 10;

        if (testCode.Contains("Mock") || testCode.Contains("Stub"))
            score += 5;

        return Math.Min(score, 100);
    }

    private bool ContainsAssertions(string testCode)
    {
        return Regex.IsMatch(testCode, @"Assert\.\w+|Should\.\w+|Expect\(");
    }

    private bool HasGoodTestName(string testCode)
    {
        return Regex.IsMatch(testCode, @"public\s+(async\s+)?(?:void|Task|Task<\w+>)\s+\w+When\w+Should\w+|Test\w+");
    }

    private string ExtractTestName(string testCode)
    {
        var match = Regex.Match(testCode, @"public\s+(?:async\s+)?(?:void|Task|Task<\w+>)\s+(\w+)\s*\(");
        return match.Success ? match.Groups[1].Value : "UnknownTest";
    }

    private decimal CalculateCoverage(string testCode)
    {
        decimal coverage = 0.5m;

        if (ContainsAssertions(testCode))
            coverage += 0.2m;

        if (testCode.Contains("Exception") || testCode.Contains("Error"))
            coverage += 0.1m;

        if (testCode.Contains("// Arrange") && testCode.Contains("// Act") && testCode.Contains("// Assert"))
            coverage += 0.1m;

        return Math.Min(coverage, 1m);
    }

    private int CalculateComplexity(string testCode)
    {
        var lines = testCode.Split('\n').Length;
        return Math.Min(lines / 5, 10);
    }

    private string GenerateRecommendation(int passed, int total, decimal avgScore, List<string> criticalIssues)
    {
        if (passed == total && avgScore >= 80)
            return "✅ Test suite is well-formed. Consider adding edge case and error scenario coverage.";

        if (avgScore < 50)
            return "⚠️ Test suite needs significant improvements. Ensure proper assertions and scenario coverage.";

        if (criticalIssues.Any())
            return $"⚠️ Address critical issues: {string.Join("; ", criticalIssues.Take(3))}";

        return "👍 Test suite is acceptable. Consider adding more comprehensive error handling tests.";
    }
}
