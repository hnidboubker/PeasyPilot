using Xunit;
using PeasyPilot.Core.Assertions;
using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Tests.AnalysisTests;

public class ReflectionTestScenarioAnalyzerTests
{
    [Fact]
    public void Analyze_WithSimpleType_ProducesProposal()
    {
        var analyzer = new ReflectionTestScenarioAnalyzer();
        var options = new TestBatteryAnalysisOptions { TargetFramework = "xunit" };

        var proposal = analyzer.Analyze(typeof(SimpleTestClass), options);

        PeasyAssert.NotNull(proposal);
        PeasyAssert.Equal("SimpleTestClass", proposal.TargetType);
        PeasyAssert.NotEmpty(proposal.TestCases);
    }

    [Fact]
    public void Analyze_ProposalIncludesTargetNamespace()
    {
        var analyzer = new ReflectionTestScenarioAnalyzer();
        var options = new TestBatteryAnalysisOptions();

        var proposal = analyzer.Analyze(typeof(SimpleTestClass), options);

        PeasyAssert.NotNull(proposal.TargetNamespace);
        PeasyAssert.Contains("PeasyPilot.TestAssistant.Tests", proposal.TargetNamespace);
    }

    // Test fixture
    public class SimpleTestClass
    {
        public string? Name { get; set; }
        public int Count { get; set; }

        public string GetName() => Name ?? "unknown";
        public int IncrementCount() => ++Count;
    }
}
