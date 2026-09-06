using System.Text.Json.Serialization;

namespace PeasyPilot.TestAssistant.Models;

public class TestCaseProposal
{
    [JsonPropertyName("methodName")]
    public required string MethodName { get; set; }

    [JsonPropertyName("testName")]
    public required string TestName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameterValues")]
    public Dictionary<string, ParameterValue> ParameterValues { get; set; } = new();

    [JsonPropertyName("expectedOutcome")]
    public string? ExpectedOutcome { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = "nominal";

    [JsonPropertyName("source")]
    public string Source { get; set; } = "mechanical";
}

public class ParameterValue
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("expression")]
    public required string Expression { get; set; }

    [JsonPropertyName("variant")]
    public string? Variant { get; set; }
}
