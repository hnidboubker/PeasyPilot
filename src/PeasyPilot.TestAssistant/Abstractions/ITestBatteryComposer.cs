using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

public interface ITestBatteryComposer
{
    TestBatteryProposal Compose(
        Type targetType,
        ConstructorResolution constructorResolution,
        IReadOnlyList<ParameterValueCase> parameterCases,
        TestBatteryAnalysisOptions options);
}
