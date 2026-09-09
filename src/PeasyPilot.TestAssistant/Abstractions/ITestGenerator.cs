using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

/// <summary>
/// Generates test code from a test plan.
/// </summary>
public interface ITestGenerator
{
    /// <summary>
    /// Gets the target test framework (xunit, nunit, tunit).
    /// </summary>
    string Framework { get; }

    /// <summary>
    /// Generates test class code from a test plan.
    /// </summary>
    string GenerateTestClass(TestPlan plan, string @namespace);

    /// <summary>
    /// Generates a single test method from a scenario.
    /// </summary>
    string GenerateTestMethod(TestableScenario scenario, MethodTestModel model);

    /// <summary>
    /// Generates test fixtures/setup code.
    /// </summary>
    string GenerateTestFixture(TestPlan plan);

    /// <summary>
    /// Generates mock/dependency setup code.
    /// </summary>
    string GenerateMockSetup(List<DependencyInfo> dependencies);
}
