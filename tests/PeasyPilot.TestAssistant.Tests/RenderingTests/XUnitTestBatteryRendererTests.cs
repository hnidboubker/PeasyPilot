using Xunit;
using PeasyPilot.TestAssistant.Rendering;
using PeasyPilot.TestAssistant.Models;
using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Tests.RenderingTests;

public class XUnitTestBatteryRendererTests
{
    [Fact]
    public void Render_ProducesValidCSharpCode()
    {
        var renderer = new XUnitTestBatteryRenderer();
        var proposal = new TestBatteryProposal
        {
            TargetType = "Calculator",
            TargetNamespace = "MyApp",
            Framework = "xunit",
            TestCases = new()
            {
                new TestCaseProposal
                {
                    MethodName = "Add",
                    TestName = "Add_WithPositiveNumbers_ReturnsSum",
                    Description = "Test adding positive numbers",
                    Category = "nominal"
                }
            }
        };

        var code = renderer.Render(proposal, new RenderOptions { OutputNamespace = "MyApp.Tests" });

        Assert.NotEmpty(code);
        Assert.Contains("using Xunit;", code);
        Assert.Contains("public class CalculatorTests", code);
        Assert.Contains("[Fact]", code);
        Assert.Contains("Add_WithPositiveNumbers_ReturnsSum", code);
    }

    [Fact]
    public void RenderKey_ReturnsXunit()
    {
        var renderer = new XUnitTestBatteryRenderer();
        Assert.Equal("xunit", renderer.RenderKey);
    }
}
