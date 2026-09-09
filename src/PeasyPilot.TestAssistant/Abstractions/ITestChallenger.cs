using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

/// <summary>
/// Validates and challenges test quality.
/// </summary>
public interface ITestChallenger
{
    /// <summary>
    /// Challenges a single test method.
    /// </summary>
    TestChallengeResult ChallengeTest(string testCode, MethodTestModel model);

    /// <summary>
    /// Generates a comprehensive challenge report for multiple tests.
    /// </summary>
    TestChallengeReport ChallengeTestSuite(List<string> testCodes, MethodTestModel model);

    /// <summary>
    /// Validates test coverage completeness.
    /// </summary>
    List<string> ValidateCoverage(TestPlan plan, List<TestableScenario> coveredScenarios);

    /// <summary>
    /// Scores test quality on multiple dimensions.
    /// </summary>
    int ScoreTestQuality(string testCode, TestableScenario scenario);
}
