using System.Text.Json;

namespace PeasyPilot.Core.Assertions;

/// <summary>
/// Provides snapshot assertion utilities for comparing complex object graphs against baseline snapshots.
/// </summary>
public static class SnapshotAssert
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Compares an actual object serialized as JSON against an expected baseline snapshot string.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    /// <param name="actual">Actual object to serialize.</param>
    /// <param name="expectedSnapshotJson">Expected baseline JSON snapshot.</param>
    /// <returns>True if actual JSON matches expected baseline; otherwise throws InvalidOperationException.</returns>
    public static bool MatchSnapshot<T>(T actual, string expectedSnapshotJson)
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(expectedSnapshotJson);

        var actualJson = JsonSerializer.Serialize(actual, JsonOptions).Replace("\r\n", "\n").Trim();
        var normalizedExpected = expectedSnapshotJson.Replace("\r\n", "\n").Trim();

        if (!string.Equals(actualJson, normalizedExpected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Snapshot match failed.\nExpected:\n{normalizedExpected}\n\nActual:\n{actualJson}");
        }

        return true;
    }
}
