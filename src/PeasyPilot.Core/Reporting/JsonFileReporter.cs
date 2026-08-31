using System.Text.Json;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;

/// <summary>
/// Formats test run results as a JSON string and optionally writes to a file.
/// </summary>
public sealed class JsonFileReporter : ITestReporter
{
    private readonly string? _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileReporter"/> class.
    /// </summary>
    /// <param name="filePath">Optional file path to save the JSON output.</param>
    public JsonFileReporter(string? filePath = null)
    {
        _filePath = filePath;
    }

    /// <inheritdoc />
    public async Task<string> ReportAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(result, options);

        if (!string.IsNullOrWhiteSpace(_filePath))
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_filePath, json, cancellationToken);
        }

        return json;
    }
}
