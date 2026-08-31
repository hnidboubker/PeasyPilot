using System.Text.Json;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Adapters;

/// <summary>
/// IDE integration protocol adapter for exporting test discovery and execution state into IDE-friendly JSON protocol payloads.
/// </summary>
public static class IdeProtocolAdapter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>
    /// Serializes discovered test cases into IDE test tree JSON.
    /// </summary>
    /// <param name="testCases">Collection of discovered test cases.</param>
    /// <returns>JSON string formatted for IDE Test Explorer protocol.</returns>
    public static string SerializeDiscovery(IEnumerable<TestCase> testCases)
    {
        ArgumentNullException.ThrowIfNull(testCases);

        var tree = testCases.Select(t => new
        {
            id = t.Name,
            label = t.Name,
            category = t.Category,
            kind = t.Kind.ToString()
        });

        return JsonSerializer.Serialize(tree, Options);
    }

    /// <summary>
    /// Serializes test execution results into IDE execution payload.
    /// </summary>
    /// <param name="results">Collection of test execution results.</param>
    /// <returns>JSON string formatted for IDE test runner status.</returns>
    public static string SerializeExecution(IEnumerable<TestResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var payload = results.Select(r => new
        {
            testName = r.Name,
            status = r.Status.ToString(),
            durationMs = r.Duration.TotalMilliseconds,
            message = r.Message
        });

        return JsonSerializer.Serialize(payload, Options);
    }
}
