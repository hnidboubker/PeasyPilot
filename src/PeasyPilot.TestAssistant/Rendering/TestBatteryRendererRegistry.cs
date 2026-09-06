using PeasyPilot.TestAssistant.Abstractions;

namespace PeasyPilot.TestAssistant.Rendering;

public class TestBatteryRendererRegistry
{
    private readonly Dictionary<string, ITestBatteryRenderer> _renderers = new()
    {
        { "xunit", new XUnitTestBatteryRenderer() },
        { "nunit", new NUnitTestBatteryRenderer() },
        { "tunit", new TUnitTestBatteryRenderer() }
    };

    public ITestBatteryRenderer GetRenderer(string framework)
    {
        if (_renderers.TryGetValue(framework.ToLowerInvariant(), out var renderer))
        {
            return renderer;
        }

        throw new ArgumentException($"Unknown test framework: {framework}. Supported: xunit, nunit, tunit");
    }
}
