using PeasyPilot.TestAssistant.Models;
namespace PeasyPilot.TestAssistant.Analysis;
public class DependencyAnalyzer
{
    public string SuggestStrategy(DependencyInfo d) => d.InterfaceName.Contains("DbContext") || d.InterfaceName.Contains("Repository") ? "IntegrationTestFixture" : "Moq.Mock";
    public bool ShouldUseFixture(DependencyInfo d) => SuggestStrategy(d) == "IntegrationTestFixture";
    public bool ShouldMock(DependencyInfo d) => SuggestStrategy(d) == "Moq.Mock";
    public Dictionary<string, List<DependencyInfo>> GroupByStrategy(List<DependencyInfo> deps)
    {
        var g = new Dictionary<string, List<DependencyInfo>>();
        foreach (var d in deps) { var s = SuggestStrategy(d); if (!g.ContainsKey(s)) g[s] = new(); g[s].Add(d); }
        return g;
    }
}
