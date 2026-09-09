using PeasyPilot.TestAssistant.Analysis;
using Xunit;
namespace PeasyPilot.Core.Tests.TestAssistant;
public class Tier1Tests
{
    private readonly CSharpCodeAnalyzer _analyzer = new();
    [Fact]
    public async Task AnalyzeTypeAsync_WithSimpleClass_ReturnsMetadata()
    {
        var result = await _analyzer.AnalyzeTypeAsync(typeof(SimpleClass));
        Assert.NotEmpty(result);
        Assert.All(result, m => Assert.NotNull(m.MethodName));
    }
    [Fact]
    public async Task AnalyzeMethodAsync_WithValidMethod_ReturnModel()
    {
        var result = await _analyzer.AnalyzeMethodAsync(typeof(SimpleClass), "Add");
        Assert.NotNull(result);
        Assert.Equal("Add", result.MethodName);
        Assert.NotEmpty(result.TestableScenarios);
    }
    public class SimpleClass { public int Add(int a, int b) => a + b; public void Print(string s) { } }
}
