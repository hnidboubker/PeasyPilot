using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;

namespace PeasyPilot.Core.Testing;

/// <summary>
/// Base class for all PeasyPilot tests with centralized logging and error tracking.
/// Automatically logs all test activity, errors, and crashes to console output visible in CI/CD.
/// </summary>
public abstract class TestLoggerBase
{
    private ILogger? _logger;
    private readonly List<TestError> _errors = new();
    private readonly Stopwatch _testStopwatch = new();

    /// <summary>
    /// Gets the test context.
    /// </summary>
    protected ITestContext TestContext { get; private set; } = null!;

    /// <summary>
    /// Gets or sets the test data factory.
    /// </summary>
    protected ITestDataFactory? TestDataFactory { get; set; }

    /// <summary>
    /// Gets or sets the mock factory.
    /// </summary>
    protected IMockFactory? MockFactory { get; set; }

    /// <summary>
    /// Gets the logger instance for this test.
    /// Automatically initialized during fixture initialization.
    /// </summary>
    protected ILogger Logger
    {
        get => _logger ?? throw new InvalidOperationException("Logger not initialized. Call InitializeLoggerAsync in InitializeAsync.");
        private set => _logger = value;
    }

    /// <summary>
    /// Gets the list of errors that occurred during this test.
    /// </summary>
    protected IReadOnlyList<TestError> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Initializes the test fixture with logging.
    /// Must be called by derived classes in their InitializeAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task InitializeLoggerAsync()
    {
        _testStopwatch.Start();
        _errors.Clear();

        // Create logger factory with console output
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole(options =>
                {
                    // Format: [HH:mm:ss.fff] [LEVEL] [Category] Message
                    options.IncludeScopes = true;
                    options.TimestampFormat = "HH:mm:ss.fff";
                    options.UseUtcTimestamp = false;
                });
        });

        Logger = loggerFactory.CreateLogger(GetType().FullName ?? "PeasyPilot.Test");

        TestContext = new TestContext();

        Logger.LogInformation("═══════════════════════════════════════════════════════════════");
        Logger.LogInformation($"Test initialized: {GetType().Name}");
        Logger.LogInformation($"Test method: {GetTestMethodName()}");
        Logger.LogInformation($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        Logger.LogInformation("═══════════════════════════════════════════════════════════════");

        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the test fixture with logging and error report.
    /// Must be called by derived classes in their DisposeAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task DisposeLoggerAsync()
    {
        _testStopwatch.Stop();

        Logger.LogInformation("─────────────────────────────────────────────────────────────────");
        Logger.LogInformation($"Test completed: {GetType().Name}");
        Logger.LogInformation($"Duration: {_testStopwatch.ElapsedMilliseconds}ms");

        if (_errors.Count > 0)
        {
            Logger.LogError($"❌ TEST FAILED: {_errors.Count} error(s) found");
            Logger.LogInformation("");
            Logger.LogInformation("📋 ERROR REPORT:");
            Logger.LogInformation("─────────────────────────────────────────────────────────────────");

            for (int i = 0; i < _errors.Count; i++)
            {
                var error = _errors[i];
                Logger.LogError($"{i + 1}. [{error.ErrorType}] {error.Message}");
                if (!string.IsNullOrEmpty(error.StackTrace))
                {
                    Logger.LogError($"   Stack: {error.StackTrace}");
                }
                Logger.LogError($"   Time: {error.Timestamp:HH:mm:ss.fff}");
                Logger.LogError("");
            }
        }
        else
        {
            Logger.LogInformation("✅ TEST PASSED: No errors detected");
        }

        Logger.LogInformation("═══════════════════════════════════════════════════════════════");

        await Task.CompletedTask;
    }

    /// <summary>
    /// Logs an assertion failure.
    /// </summary>
    /// <param name="message">Description of the assertion failure.</param>
    /// <param name="expected">Expected value.</param>
    /// <param name="actual">Actual value.</param>
    protected void LogAssertionFailure(string message, object? expected = null, object? actual = null)
    {
        var errorMessage = $"Assertion failed: {message}";
        if (expected != null && actual != null)
        {
            errorMessage += $" (Expected: {expected}, Actual: {actual})";
        }

        Logger.LogError($"❌ {errorMessage}");

        var error = new TestError
        {
            ErrorType = "AssertionFailure",
            Message = errorMessage,
            StackTrace = GetStackTrace(),
            Timestamp = DateTime.Now
        };
        _errors.Add(error);
    }

    /// <summary>
    /// Logs an exception that occurred during the test.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">Optional additional context message.</param>
    protected void LogException(Exception ex, string? message = null)
    {
        var errorMessage = message ?? ex.Message;
        Logger.LogError($"💥 EXCEPTION: {ex.GetType().Name}: {errorMessage}");
        Logger.LogError($"Details: {ex}");

        var error = new TestError
        {
            ErrorType = ex.GetType().Name,
            Message = errorMessage,
            StackTrace = ex.StackTrace,
            Timestamp = DateTime.Now
        };
        _errors.Add(error);
    }

    /// <summary>
    /// Logs a warning that might indicate a problem.
    /// </summary>
    /// <param name="message">Warning message.</param>
    protected void LogWarning(string message)
    {
        Logger.LogWarning($"⚠️  {message}");
    }

    /// <summary>
    /// Gets a formatted error report for the test.
    /// </summary>
    /// <returns>Error report as string.</returns>
    protected string GetErrorReport()
    {
        if (_errors.Count == 0)
            return "✅ No errors detected";

        var sb = new StringBuilder();
        sb.AppendLine($"❌ TEST ERRORS ({_errors.Count} total):");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");

        for (int i = 0; i < _errors.Count; i++)
        {
            var error = _errors[i];
            sb.AppendLine($"{i + 1}. [{error.ErrorType}] {error.Message}");
            if (!string.IsNullOrEmpty(error.StackTrace))
            {
                sb.AppendLine($"   Stack trace: {error.StackTrace}");
            }
            sb.AppendLine($"   Time: {error.Timestamp:HH:mm:ss.fff}");
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        return sb.ToString();
    }

    /// <summary>
    /// Gets or creates test data from the context.
    /// </summary>
    /// <typeparam name="T">The type of test data.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function.</param>
    /// <returns>The test data.</returns>
    protected T GetOrCreateTestData<T>(string key, Func<T> factory) where T : class
    {
        Logger.LogDebug($"Getting or creating test data: {key}");
        return TestContext.GetOrAdd(key, factory);
    }

    /// <summary>
    /// Gets the name of the current test method from the call stack.
    /// </summary>
    /// <returns>The test method name, or "Unknown" if not found.</returns>
    private static string GetTestMethodName()
    {
        var stackTrace = new System.Diagnostics.StackTrace();
        foreach (var frame in stackTrace.GetFrames() ?? Array.Empty<System.Diagnostics.StackFrame>())
        {
            var method = frame.GetMethod();
            if (method?.DeclaringType?.Name.EndsWith("Tests") == true)
            {
                return method.Name;
            }
        }
        return "Unknown";
    }

    /// <summary>
    /// Gets the current stack trace as a formatted string.
    /// </summary>
    /// <returns>Stack trace string.</returns>
    private static string GetStackTrace()
    {
        var st = new System.Diagnostics.StackTrace(skipFrames: 2, fNeedFileInfo: true);
        return st.ToString().Trim();
    }
}

/// <summary>
/// Represents an error that occurred during test execution.
/// </summary>
public class TestError
{
    /// <summary>
    /// Type of error (AssertionFailure, Exception name, etc).
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Error message or description.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Full stack trace of the error.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Timestamp when error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
