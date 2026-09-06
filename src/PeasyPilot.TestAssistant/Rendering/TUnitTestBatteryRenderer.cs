using PeasyPilot.TestAssistant.Abstractions;
using PeasyPilot.TestAssistant.Models;
using System.Text;

namespace PeasyPilot.TestAssistant.Rendering;

public class TUnitTestBatteryRenderer : ITestBatteryRenderer
{
    public string RenderKey => "tunit";

    public string Render(TestBatteryProposal proposal, RenderOptions options)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using PeasyPilot.TUnit;");
        sb.AppendLine("using PeasyPilot.Moq;");
        sb.AppendLine();
        sb.AppendLine($"namespace {options.OutputNamespace ?? proposal.TargetNamespace}.Tests;");
        sb.AppendLine();
        sb.AppendLine($"public class {proposal.TargetType}Tests : PeasyPilotTUnitTestBase");
        sb.AppendLine("{");
        sb.AppendLine($"{options.Indent}private {proposal.TargetType} _subject = null!;");
        sb.AppendLine();
        sb.AppendLine($"{options.Indent}public override async Task BeforeEachAsync()");
        sb.AppendLine($"{options.Indent}{{");
        sb.AppendLine($"{options.Indent}{options.Indent}await base.BeforeEachAsync();");
        sb.AppendLine($"{options.Indent}{options.Indent}_subject = new {proposal.TargetType}();");
        sb.AppendLine($"{options.Indent}}}");
        sb.AppendLine();

        foreach (var testCase in proposal.TestCases)
        {
            sb.AppendLine($"{options.Indent}public async Task {testCase.TestName}()");
            sb.AppendLine($"{options.Indent}{{");
            sb.AppendLine($"{options.Indent}{options.Indent}// {testCase.Description}");
            sb.AppendLine($"{options.Indent}{options.Indent}// TODO: Implement test");
            sb.AppendLine($"{options.Indent}{options.Indent}await Assert.That(_subject).IsNotNull();");
            sb.AppendLine($"{options.Indent}}}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
