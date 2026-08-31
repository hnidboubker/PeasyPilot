using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.ImpactAnalysis;

/// <summary>
/// Impact analyzer that matches changed Git source files and code symbol paths against registered TestCase metadata.
/// </summary>
public sealed class GitAstImpactAnalyzer : ITestImpactAnalyzer
{
    /// <inheritdoc />
    public Task<IReadOnlyCollection<TestCase>> GetImpactedTestsAsync(
        IEnumerable<string> changedFiles,
        IEnumerable<TestCase> tests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changedFiles);
        ArgumentNullException.ThrowIfNull(tests);
        cancellationToken.ThrowIfCancellationRequested();

        var changedList = changedFiles.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        var testList = tests.ToList();

        var impacted = testList.Where(test =>
        {
            foreach (var changed in changedList)
            {
                if (string.IsNullOrEmpty(changed)) continue;

                if (test.Name.Contains(changed, StringComparison.OrdinalIgnoreCase) ||
                    test.Category.Contains(changed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                foreach (var meta in test.Metadata.Values)
                {
                    if (meta.Contains(changed, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }).ToList();

        return Task.FromResult<IReadOnlyCollection<TestCase>>(impacted);
    }
}
