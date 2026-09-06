namespace PeasyPilot.TestAssistant.Models;

public class TestBatteryAnalysisOptions
{
    public int MaxEnumCases { get; set; } = 8;

    public bool IncludeAsyncVariants { get; set; } = true;

    public bool GenerateNullCases { get; set; } = true;

    public string TargetFramework { get; set; } = "xunit";
}
