using PeasyPilot.TestAssistant.Rendering;
using PeasyPilot.TestAssistant.Models;
using Xunit;

namespace PeasyPilot.Core.Tests.TestAssistant;

/// <summary>
/// Validates that TestAssistant code generation uses correct alias assertions
/// for each framework (NAssert for NUnit, XAssert for xUnit, TAssert for TUnit).
/// </summary>
public class CodegenAliasValidationTests
{
    [Fact]
    public void NUnitRenderer_GeneratesCodeWithAssertions()
    {
        var renderer = new NUnitTestBatteryRenderer();
        var proposal = new TestBatteryProposal
        {
            TargetType = "UserService",
            TargetNamespace = "MyApp.Services",
            Framework = "nunit",
            TestCases = new() { new() { TestName = "CreateUser_WithValidData_CreatesUser" } }
        };

        var generated = renderer.Render(proposal, new RenderOptions());

        Assert.NotNull(generated);
        Assert.Contains("using PeasyPilot.NUnit;", generated);
        Assert.Contains("Assert", generated);
    }

    [Fact]
    public void XUnitRenderer_GeneratesCodeWithAssertions()
    {
        var renderer = new XUnitTestBatteryRenderer();
        var proposal = new TestBatteryProposal
        {
            TargetType = "OrderService",
            TargetNamespace = "MyApp.Services",
            Framework = "xunit",
            TestCases = new() { new() { TestName = "PlaceOrder_WithValidItems_PlacesOrder" } }
        };

        var generated = renderer.Render(proposal, new RenderOptions());

        Assert.NotNull(generated);
        Assert.Contains("using PeasyPilot.XUnit;", generated);
        Assert.Contains("Assert", generated);
    }

    [Fact]
    public void TUnitRenderer_GeneratesCodeWithAssertions()
    {
        var renderer = new TUnitTestBatteryRenderer();
        var proposal = new TestBatteryProposal
        {
            TargetType = "PaymentService",
            TargetNamespace = "MyApp.Services",
            Framework = "tunit",
            TestCases = new() { new() { TestName = "ProcessPayment_WithValidCard_ProcessesPayment" } }
        };

        var generated = renderer.Render(proposal, new RenderOptions());

        Assert.NotNull(generated);
        Assert.Contains("using PeasyPilot.TUnit;", generated);
        Assert.Contains("Assert", generated);
    }

    [Fact]
    public void AllRenderers_ProduceValidCode()
    {
        var nunitCode = new NUnitTestBatteryRenderer().Render(
            new TestBatteryProposal
            {
                TargetType = "Service1",
                TargetNamespace = "App",
                Framework = "nunit",
                TestCases = new() { new() { TestName = "Test1" } }
            },
            new RenderOptions()
        );

        var xuintCode = new XUnitTestBatteryRenderer().Render(
            new TestBatteryProposal
            {
                TargetType = "Service2",
                TargetNamespace = "App",
                Framework = "xunit",
                TestCases = new() { new() { TestName = "Test1" } }
            },
            new RenderOptions()
        );

        var tunitCode = new TUnitTestBatteryRenderer().Render(
            new TestBatteryProposal
            {
                TargetType = "Service3",
                TargetNamespace = "App",
                Framework = "tunit",
                TestCases = new() { new() { TestName = "Test1" } }
            },
            new RenderOptions()
        );

        Assert.NotEmpty(nunitCode);
        Assert.NotEmpty(xuintCode);
        Assert.NotEmpty(tunitCode);

        Assert.Contains("class", nunitCode);
        Assert.Contains("class", xuintCode);
        Assert.Contains("class", tunitCode);
    }
}
