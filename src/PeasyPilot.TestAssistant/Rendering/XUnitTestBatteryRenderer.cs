using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
using System.Text;

namespace PeasyPilot.TestAssistant.Rendering;

public class XUnitTestBatteryRenderer : ITestBatteryRenderer
{
    public string RenderKey => "xunit";

    public string Render(TestBatteryProposal proposal, RenderOptions options)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using Xunit;");
        sb.AppendLine("using PeasyPilot.XUnit;");
        sb.AppendLine("using PeasyPilot.Moq;");
        sb.AppendLine();
        sb.AppendLine($"namespace {options.OutputNamespace ?? proposal.TargetNamespace}.Tests;");
        sb.AppendLine();
        sb.AppendLine($"public class {proposal.TargetType}Tests : PeasyPilotTestBase");
        sb.AppendLine("{");
        sb.AppendLine($"{options.Indent}private {proposal.TargetType} _subject = null!;");
        sb.AppendLine();
        sb.AppendLine($"{options.Indent}public override void Setup()");
        sb.AppendLine($"{options.Indent}{{");
        sb.AppendLine($"{options.Indent}{options.Indent}base.Setup();");
        sb.AppendLine($"{options.Indent}{options.Indent}_subject = new {proposal.TargetType}();");
        sb.AppendLine($"{options.Indent}}}");
        sb.AppendLine();

        foreach (var testCase in proposal.TestCases)
        {
            sb.AppendLine($"{options.Indent}[Fact]");
            sb.AppendLine($"{options.Indent}public void {testCase.TestName}()");
            sb.AppendLine($"{options.Indent}{{");
            sb.AppendLine($"{options.Indent}{options.Indent}// {testCase.Description}");
            sb.AppendLine($"{options.Indent}{options.Indent}// TODO: Implement test");
            sb.AppendLine($"{options.Indent}{options.Indent}Assert.NotNull(_subject);");
            sb.AppendLine($"{options.Indent}}}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
