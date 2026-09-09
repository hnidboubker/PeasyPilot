namespace PeasyPilot.Mcp.Models;

/// <summary>
/// Response from RunTestsTool
/// </summary>
public class RunTestsResponse
{
#pragma warning disable IDE1006 // Naming Conventions
    public required bool success { get; set; }
    public int total { get; set; }
    public int passed { get; set; }
    public int failed { get; set; }
    public List<object>? failures { get; set; }
    public double durationMs { get; set; }
    public string? summary { get; set; }
    public string? error { get; set; }
#pragma warning restore IDE1006
}
