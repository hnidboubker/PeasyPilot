namespace PeasyPilot.Core.ImpactAnalysis;

using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

/// <summary>
/// Default implementation that matches changed items to test names.
/// </summary>
public sealed class DefaultTestImpactAnalyzer : ITestImpactAnalyzer
{
    /// <inheritdoc />
    public Task<IReadOnlyCollection<TestCase>> GetImpactedTestsAsync(IEnumerable<string> changedFiles, IEnumerable<TestCase> tests, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changedFiles);
        ArgumentNullException.ThrowIfNull(tests);
        cancellationToken.ThrowIfCancellationRequested();

        var changed = changedFiles
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var impacted = tests
            .Where(test =>
            {
                if (changed.Count == 0)
                {
                    return true;
                }

                var haystacks = new[]
                {
                    test.Name,
                    test.Category,
                    test.Metadata.TryGetValue("File", out var file) ? file : string.Empty
                };

                return haystacks.Any(haystack =>
                    changed.Any(change => haystack.Contains(change, StringComparison.OrdinalIgnoreCase)));
            })
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<TestCase>>(impacted);
    }
}
