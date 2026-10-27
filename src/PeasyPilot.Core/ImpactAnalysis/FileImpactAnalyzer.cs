using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.ImpactAnalysis;

/// <summary>
/// Selects tests from the files changed in a commit, using each test's source file (<c>File</c> metadata).
/// Rules: no changed file selects every test; a test without a known file is always selected; a test whose file
/// changed is selected; a test file named after a changed file (<c>OrderServiceTests.cs</c> for <c>OrderService.cs</c>) is selected.
/// </summary>
public sealed class FileImpactAnalyzer : ITestImpactAnalyzer
{
    private const string FileMetadataKey = "File";
    private static readonly string[] TestSuffixes = ["Tests", "Test"];

    /// <inheritdoc />
    public Task<IReadOnlyCollection<TestCase>> GetImpactedTestsAsync(
        IEnumerable<string> changedFiles,
        IEnumerable<TestCase> tests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changedFiles);
        ArgumentNullException.ThrowIfNull(tests);
        cancellationToken.ThrowIfCancellationRequested();

        var changed = changedFiles
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Normalize)
            .ToList();
        var all = tests.ToList();

        if (changed.Count == 0)
        {
            return Task.FromResult<IReadOnlyCollection<TestCase>>(all);
        }

        var impacted = all.Where(test => IsImpacted(test, changed)).ToList();
        return Task.FromResult<IReadOnlyCollection<TestCase>>(impacted);
    }

    private static bool IsImpacted(TestCase test, IReadOnlyList<string> changed)
    {
        if (!test.Metadata.TryGetValue(FileMetadataKey, out var file) || string.IsNullOrWhiteSpace(file))
        {
            return true;
        }

        var testFile = Normalize(file);
        if (changed.Any(path => testFile == path || testFile.EndsWith("/" + path, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var testStem = StripTestSuffix(Path.GetFileNameWithoutExtension(testFile));
        return changed.Any(path =>
            string.Equals(testStem, Path.GetFileNameWithoutExtension(path), StringComparison.OrdinalIgnoreCase));
    }

    private static string StripTestSuffix(string stem)
    {
        foreach (var suffix in TestSuffixes)
        {
            if (stem.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) && stem.Length > suffix.Length)
            {
                return stem[..^suffix.Length];
            }
        }

        return stem;
    }

    private static string Normalize(string path) => path.Trim().Replace('\\', '/');
}
