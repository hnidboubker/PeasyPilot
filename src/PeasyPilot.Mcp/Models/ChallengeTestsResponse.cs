namespace PeasyPilot.Mcp.Models;

/// <summary>
/// Response from ChallengeTestsTool
/// </summary>
public class ChallengeTestsResponse
{
#pragma warning disable IDE1006 // Naming Conventions
    public required bool success { get; set; }
    public string? method { get; set; }
    public int qualityScore { get; set; }
    public List<string>? issues { get; set; }
    public List<string>? suggestions { get; set; }
    public List<string>? missingScenarios { get; set; }
    public decimal estimatedCoverage { get; set; }
    public bool isHealthy { get; set; }
    public string? error { get; set; }
#pragma warning restore IDE1006
}
