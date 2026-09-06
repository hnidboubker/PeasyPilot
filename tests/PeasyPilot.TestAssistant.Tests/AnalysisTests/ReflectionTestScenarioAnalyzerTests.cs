using Xunit;
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

        Assert.NotNull(proposal);
        Assert.Equal("SimpleTestClass", proposal.TargetType);
        Assert.NotEmpty(proposal.TestCases);
    }

    [Fact]
    public void Analyze_ProposalIncludesTargetNamespace()
    {
        var analyzer = new ReflectionTestScenarioAnalyzer();
        var options = new TestBatteryAnalysisOptions();

        var proposal = analyzer.Analyze(typeof(SimpleTestClass), options);

        Assert.NotNull(proposal.TargetNamespace);
        Assert.Contains("PeasyPilot.TestAssistant.Tests", proposal.TargetNamespace);
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
