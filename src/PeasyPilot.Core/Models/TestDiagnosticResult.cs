namespace PeasyPilot.Core.Models;

/// <summary>
/// Provides a framework-agnostic diagnosis for a failed test result.
/// </summary>
public sealed record TestDiagnosticResult(
    string Summary,
    string? RootCause,
    IReadOnlyCollection<string> RelatedTests,
    IReadOnlyCollection<string> Suggestions);
