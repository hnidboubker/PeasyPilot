using Microsoft.Extensions.Logging;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Context;

namespace PeasyPilot.Core.Testing;

/// <summary>
/// Base class for all PeasyPilot tests with centralized logging.
/// Automatically logs all test activity to console output visible in CI/CD.
/// </summary>
public abstract class TestLoggerBase
{
    private ILogger? _logger;

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
    /// Initializes the test fixture with logging.
    /// Must be called by derived classes in their InitializeAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task InitializeLoggerAsync()
    {
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
        Logger.LogInformation("═══════════════════════════════════════════════════════════════");

        await Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the test fixture with logging.
    /// Must be called by derived classes in their DisposeAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task DisposeLoggerAsync()
    {
        Logger.LogInformation("─────────────────────────────────────────────────────────────────");
        Logger.LogInformation($"Test completed: {GetType().Name}");
        Logger.LogInformation("═══════════════════════════════════════════════════════════════");

        await Task.CompletedTask;
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
            if (method?.DeclaringType?.Name.EndsWith("Tests") == true ||
                method?.GetCustomAttributes(typeof(Xunit.FactAttribute), false).Length > 0 ||
                method?.GetCustomAttributes(typeof(Xunit.TheoryAttribute), false).Length > 0)
            {
                return method.Name;
            }
        }
        return "Unknown";
    }
}
