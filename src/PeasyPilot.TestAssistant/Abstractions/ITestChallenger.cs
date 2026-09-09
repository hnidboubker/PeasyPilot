using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

/// <summary>
/// Validates and challenges generated test code for quality and completeness.
/// </summary>
public interface ITestChallenger
{
    /// <summary>
    /// Challenge a single test code for quality issues and suggestions.
    /// </summary>
    TestChallengeResult ChallengeTest(string testCode, MethodTestModel model);

    /// <summary>
    /// Challenge an entire test suite for consistency and coverage.
    /// </summary>
    TestChallengeReport ChallengeTestSuite(List<string> testCodes, MethodTestModel model);

    /// <summary>
    /// Validate coverage against planned scenarios.
    /// </summary>
    List<string> ValidateCoverage(TestPlan plan, List<TestableScenario> coveredScenarios);

    /// <summary>
    /// Score test quality based on pattern and structure.
    /// </summary>
    int ScoreTestQuality(string testCode, TestableScenario scenario);
}
