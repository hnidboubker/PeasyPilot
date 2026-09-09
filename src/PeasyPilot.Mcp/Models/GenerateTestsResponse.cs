namespace PeasyPilot.Mcp.Models;

/// <summary>
/// Response from GenerateTestsTool
/// </summary>
public class GenerateTestsResponse
{
#pragma warning disable IDE1006 // Naming Conventions
    public required bool success { get; set; }
    public string? framework { get; set; }
    public string? method { get; set; }
    public string? generatedCode { get; set; }
    public List<object>? scenarios { get; set; }
    public int estimatedTestCount { get; set; }
    public int riskScore { get; set; }
    public object? quality { get; set; }
    public string? error { get; set; }
#pragma warning restore IDE1006
}
