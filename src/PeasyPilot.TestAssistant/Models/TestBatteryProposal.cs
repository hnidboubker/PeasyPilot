using System.Text.Json.Serialization;

namespace PeasyPilot.TestAssistant.Models;

public class TestBatteryProposal
{
    [JsonPropertyName("targetType")]
    public required string TargetType { get; set; }

    [JsonPropertyName("targetNamespace")]
    public required string TargetNamespace { get; set; }

    [JsonPropertyName("framework")]
    public required string Framework { get; set; }

    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("testCases")]
    public List<TestCaseProposal> TestCases { get; set; } = new();

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}
