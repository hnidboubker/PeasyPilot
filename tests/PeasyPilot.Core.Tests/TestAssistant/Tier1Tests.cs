using PeasyPilot.TestAssistant.Analysis;
using PeasyPilot.XUnit;

using Xunit;
namespace PeasyPilot.Core.Tests.TestAssistant;
public class Tier1Tests
{
    private readonly CSharpCodeAnalyzer _analyzer = new();
    [Fact]
    public async Task AnalyzeTypeAsync_WithSimpleClass_ReturnsMetadata()
    {
        var result = await _analyzer.AnalyzeTypeAsync(typeof(SimpleClass));
        XAssert.NotEmpty(result);
        // Todo Add this methode All it'doesn't exists
        XAssert.All(result, m => XAssert.NotNull(m.MethodName));
    }
    [Fact]
    public async Task AnalyzeMethodAsync_WithValidMethod_ReturnModel()
    {
        var result = await _analyzer.AnalyzeMethodAsync(typeof(SimpleClass), "Add");
        XAssert.NotNull(result);
        // Todo fix this  result
        if (result != null)
        {
            XAssert.Equal(expected: "Add", actual: result.MethodName);
            XAssert.NotEmpty(result.TestableScenarios);
        }
       
    }
    public class SimpleClass { public int Add(int a, int b) => a + b; public void Print(string s) { } }
}
