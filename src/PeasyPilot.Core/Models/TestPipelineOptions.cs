using PeasyPilot.Core.Abstractions;

namespace PeasyPilot.Core.Models;

/// <summary>
/// Options for configuring an end-to-end test execution pipeline.
/// </summary>
public sealed class TestPipelineOptions
{
    /// <summary>
    /// Gets or sets the list of changed files for test impact analysis.
    /// If null or empty, impact analysis is skipped.
    /// </summary>
    public IReadOnlyCollection<string>? ChangedFiles { get; set; }

    /// <summary>
    /// Gets or sets an optional test filter.
    /// </summary>
    public ITestFilter? Filter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether diagnostics should be run when tests fail.
    /// </summary>
    public bool RunDiagnosticsOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of test reporters.
    /// </summary>
    public IReadOnlyCollection<ITestReporter> Reporters { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of diagnostic providers.
    /// </summary>
    public IReadOnlyCollection<ITestDiagnostic> Diagnostics { get; set; } = [];
}
