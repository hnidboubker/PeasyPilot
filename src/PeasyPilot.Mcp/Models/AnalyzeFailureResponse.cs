namespace PeasyPilot.Mcp.Models;

/// <summary>
/// Response from AnalyzeFailureTool
/// </summary>
public class AnalyzeFailureResponse
{
#pragma warning disable IDE1006 // Naming Conventions
    public required bool success { get; set; }
    public string? testName { get; set; }
    public string? failureType { get; set; }
    public string? reason { get; set; }
    public List<string>? suggestions { get; set; }
    public string? riskLevel { get; set; }
    public string? error { get; set; }
#pragma warning restore IDE1006
}
