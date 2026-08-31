using System.Text.Json;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Storage;

/// <summary>
/// File-based implementation of <see cref="ITestRunStore"/> for persisting test execution history as JSON files.
/// </summary>
public sealed class FileTestRunStore : ITestRunStore
{
    private readonly string _storageDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTestRunStore"/> class.
    /// </summary>
    /// <param name="storageDirectory">The directory path to store test run history files.</param>
    public FileTestRunStore(string? storageDirectory = null)
    {
        _storageDirectory = storageDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), ".peasypilot", "history");
    }

    /// <inheritdoc />
    public async Task SaveRunAsync(TestRunRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(_storageDirectory);

        var fileName = $"run_{record.ExecutedAt:yyyyMMdd_HHmmss}_{record.RunId}.json";
        var filePath = Path.Combine(_storageDirectory, fileName);

        var json = JsonSerializer.Serialize(record, JsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TestRunRecord>> GetRunHistoryAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_storageDirectory))
        {
            return Array.Empty<TestRunRecord>();
        }

        var files = Directory.GetFiles(_storageDirectory, "run_*.json");
        var list = new List<TestRunRecord>();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var record = JsonSerializer.Deserialize<TestRunRecord>(json, JsonOptions);
                if (record != null)
                {
                    list.Add(record);
                }
            }
            catch
            {
                // Skip corrupted history files
            }
        }

        var ordered = list.OrderByDescending(r => r.ExecutedAt);
        return limit.HasValue ? ordered.Take(limit.Value).ToList() : ordered.ToList();
    }

    /// <inheritdoc />
    public async Task<TestRunRecord?> GetLatestRunAsync(CancellationToken cancellationToken = default)
    {
        var history = await GetRunHistoryAsync(1, cancellationToken);
        return history.FirstOrDefault();
    }
}
