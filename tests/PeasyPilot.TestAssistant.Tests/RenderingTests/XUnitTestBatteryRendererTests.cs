using Xunit;
using PeasyPilot.Core.Assertions;
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

        PeasyAssert.NotEmpty(code);
        PeasyAssert.Contains("using Xunit;", code);
        PeasyAssert.Contains("public class CalculatorTests", code);
        PeasyAssert.Contains("[Fact]", code);
        PeasyAssert.Contains("Add_WithPositiveNumbers_ReturnsSum", code);
    }

    [Fact]
    public void RenderKey_ReturnsXunit()
    {
        var renderer = new XUnitTestBatteryRenderer();
        PeasyAssert.Equal("xunit", renderer.RenderKey);
    }

    [Fact]
    public void Render_GeneratesAsyncInitializeAsyncMethod()
    {
        var renderer = new XUnitTestBatteryRenderer();
        var proposal = new TestBatteryProposal
        {
            TargetType = "StringUtils",
            TargetNamespace = "MyApp",
            Framework = "xunit",
            TestCases = new()
            {
                new TestCaseProposal
                {
                    MethodName = "Concat",
                    TestName = "Concat_WithTwoStrings_ReturnsCombined",
                    Description = "Test string concatenation",
                    Category = "nominal"
                }
            }
        };

        var code = renderer.Render(proposal, new RenderOptions { OutputNamespace = "MyApp.Tests" });

        PeasyAssert.Contains("public override async Task InitializeAsync()", code);
        PeasyAssert.Contains("await base.InitializeAsync();", code);
        PeasyAssert.Contains("_subject = new StringUtils();", code);
        PeasyAssert.DoesNotContain("public override void Setup()", code);
    }
}
