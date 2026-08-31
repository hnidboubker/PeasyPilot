using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Discovery;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Orchestration;
using PeasyPilot.Core.Reporting;
using PeasyPilot.Core.Scheduling;
using PeasyPilot.Core.Storage;

namespace PeasyPilot.Core;

/// <summary>
/// Unified Enterprise Testing Platform entry point for PeasyPilot v1.0.
/// Provides access to pipeline orchestration, smart diagnostics, historical storage, and reporters.
/// </summary>
public sealed class PeasyPilotPlatform
{
    private static readonly Lazy<PeasyPilotPlatform> LazyInstance = new(() => new PeasyPilotPlatform());

    /// <summary>
    /// Gets the global shared instance of the PeasyPilot Platform.
    /// </summary>
    public static PeasyPilotPlatform Instance => LazyInstance.Value;

    /// <summary>
    /// Gets the platform version string.
    /// </summary>
    public string Version => "1.0.0-enterprise";

    /// <summary>
    /// Gets the pipeline orchestrator.
    /// </summary>
    public ITestPipelineOrchestrator Pipeline { get; }

    /// <summary>
    /// Gets the in-memory test run history store.
    /// </summary>
    public ITestRunStore RunStore { get; }

    /// <summary>
    /// Gets the in-memory entity test store.
    /// </summary>
    public ITestStore EntityStore { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PeasyPilotPlatform"/> class.
    /// </summary>
    public PeasyPilotPlatform()
    {
        var discovery = new DefaultTestDiscovery();
        var scheduler = new SmartParallelScheduler();
        RunStore = new InMemoryTestRunStore();
        EntityStore = new InMemoryTestStore();

        Pipeline = new TestPipelineOrchestrator(discovery, scheduler);
    }

    /// <summary>
    /// Executes a full test pipeline with options and stores the execution record in history.
    /// </summary>
    /// <param name="options">Pipeline configuration options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The aggregated pipeline result.</returns>
    public async Task<TestPipelineResult> ExecuteAsync(TestPipelineOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var result = await Pipeline.ExecutePipelineAsync(options, cancellationToken);

        await RunStore.SaveRunAsync(new TestRunRecord
        {
            ExecutedAt = DateTime.UtcNow,
            PipelineResult = result
        }, cancellationToken);

        return result;
    }
}
