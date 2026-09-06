using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

public interface ITestScenarioAnalyzer
{
    TestBatteryProposal Analyze(Type targetType, TestBatteryAnalysisOptions options);
}
