using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.Core.Assertions;
using Xunit;
namespace PeasyPilot.Core.Tests.TestAssistant;
public class Tier1Tests
{
    private readonly CSharpCodeAnalyzer _analyzer = new();
    [Fact]
    public async Task AnalyzeTypeAsync_WithSimpleClass_ReturnsMetadata()
    {
        var result = await _analyzer.AnalyzeTypeAsync(typeof(SimpleClass));
        PeasyAssert.NotEmpty(result);
        // Todo Add this methode All it'doesn't exists
        PeasyAssert.All(result, m => PeasyAssert.NotNull(m.MethodName));
    }
    [Fact]
    public async Task AnalyzeMethodAsync_WithValidMethod_ReturnModel()
    {
        var result = await _analyzer.AnalyzeMethodAsync(typeof(SimpleClass), "Add");
        PeasyAssert.NotNull(result);
        // Todo fix this  result
        if (result != null)
        {
            PeasyAssert.Equal(expected: "Add", actual: result.MethodName);
            PeasyAssert.NotEmpty(result.TestableScenarios);
        }
       
    }
    public class SimpleClass { public int Add(int a, int b) => a + b; public void Print(string s) { } }
}
