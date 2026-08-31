using System.Collections.Concurrent;

namespace PeasyPilot.Core.Diagnostics;

/// <summary>
/// Thread-safe in-memory buffer for capturing execution logs and output messages associated with test runs.
/// </summary>
public sealed class TestLogCapture
{
    private readonly ConcurrentQueue<string> _logs = new();

    /// <summary>
    /// Writes a log message to the capture buffer.
    /// </summary>
    /// <param name="message">Log message string.</param>
    public void WriteLog(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            _logs.Enqueue($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
        }
    }

    /// <summary>
    /// Gets all captured log entries.
    /// </summary>
    /// <returns>Collection of formatted log strings.</returns>
    public IReadOnlyCollection<string> GetLogs()
    {
        return _logs.ToList();
    }

    /// <summary>
    /// Clears the log buffer.
    /// </summary>
    public void Clear()
    {
        _logs.Clear();
    }
}
