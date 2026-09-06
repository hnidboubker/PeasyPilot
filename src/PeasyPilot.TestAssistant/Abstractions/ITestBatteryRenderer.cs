using PeasyPilot.TestAssistant.Models;

namespace PeasyPilot.TestAssistant.Abstractions;

public interface ITestBatteryRenderer
{
    string RenderKey { get; }

    string Render(TestBatteryProposal proposal, RenderOptions options);
}

public class RenderOptions
{
    public string? OutputNamespace { get; set; }

    public bool IncludeUsings { get; set; } = true;

    public string Indent { get; set; } = "    ";
}
