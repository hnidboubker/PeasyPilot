using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

public interface ITestPlanBuilder
{
    /// <summary>
    /// Builds a comprehensive test plan from analyzed method metadata.
    /// </summary>
    TestPlan BuildPlan(MethodTestModel analysis);

    /// <summary>
    /// Enriches an existing plan with dependency-based strategy recommendations.
    /// </summary>
    TestPlan EnrichPlan(TestPlan plan, List<DependencyInfo> dependencies);
}
