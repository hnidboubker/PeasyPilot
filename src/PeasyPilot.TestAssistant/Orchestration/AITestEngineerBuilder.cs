using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Generation;
using PeasyPilot.TestAssistant.Planning;
using PeasyPilot.TestAssistant.Validation;

namespace PeasyPilot.TestAssistant.Orchestration;

/// <summary>
/// Fluent builder for configuring AITestEngineer.
/// </summary>
public class AITestEngineerBuilder
{
    private ICodeAnalyzer? _analyzer;
    private ITestPlanBuilder? _planner;
    private TestGeneratorRegistry? _generatorRegistry;
    private ITestChallenger? _challenger;

    public AITestEngineerBuilder WithAnalyzer(ICodeAnalyzer analyzer)
    {
        _analyzer = analyzer;
        return this;
    }

    public AITestEngineerBuilder WithPlanner(ITestPlanBuilder planner)
    {
        _planner = planner;
        return this;
    }

    public AITestEngineerBuilder WithGenerators(TestGeneratorRegistry registry)
    {
        _generatorRegistry = registry;
        return this;
    }

    public AITestEngineerBuilder WithChallenger(ITestChallenger challenger)
    {
        _challenger = challenger;
        return this;
    }

    public AITestEngineerBuilder UseDefaults()
    {
        _analyzer ??= new CSharpCodeAnalyzer();
        _planner ??= new TestPlanBuilder();
        _generatorRegistry ??= new TestGeneratorRegistry();
        _challenger ??= new TestChallenger();
        return this;
    }

    public IAITestEngineer Build()
    {
        UseDefaults();

        return new AITestEngineer(
            _analyzer!,
            _planner!,
            _generatorRegistry!,
            _challenger!);
    }

    public static AITestEngineerBuilder CreateDefault() => new AITestEngineerBuilder().UseDefaults();
}
