using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Abstractions;

/// <summary>
/// Defines the contract for an end-to-end test execution pipeline orchestrator.
/// </summary>
public interface ITestPipelineOrchestrator
{
    /// <summary>
    /// Executes the complete test pipeline (Discovery -> Impact Analysis -> Filter -> Schedule -> Report -> Diagnose).
    /// </summary>
    /// <param name="options">The options configuring the pipeline run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the pipeline execution.</returns>
    Task<TestPipelineResult> ExecutePipelineAsync(TestPipelineOptions options, CancellationToken cancellationToken = default);
}
