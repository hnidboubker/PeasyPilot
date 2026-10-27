using PeasyPilot.Core.ImpactAnalysis;
using PeasyPilot.Core.Models;
using PeasyPilot.TestAssistant.Catalog;

namespace PeasyPilot.CLI;

/// <summary>
/// Chooses which test executables to run for a set of changed files.
/// An executable is kept when its project contains an impacted test, or when that cannot be known
/// (no project found, or no test found in its sources). If nothing is kept, every executable runs.
/// </summary>
public static class AssemblyImpactSelector
{
    /// <summary>
    /// Selects the executables to run.
    /// </summary>
    /// <param name="assemblies">The test executables given on the command line.</param>
    /// <param name="changedFiles">Repository-relative paths changed since the reference.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The executables to run.</returns>
    public static async Task<IReadOnlyList<string>> SelectAsync(
        IReadOnlyList<string> assemblies,
        IReadOnlyList<string> changedFiles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        ArgumentNullException.ThrowIfNull(changedFiles);

        if (changedFiles.Count == 0)
        {
            return assemblies;
        }

        var analyzer = new FileImpactAnalyzer();
        var selected = new List<string>();

        foreach (var assembly in assemblies)
        {
            var projectDirectory = FindProjectDirectory(assembly);
            if (projectDirectory is null)
            {
                selected.Add(assembly);
                continue;
            }

            var sources = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(file => !IsBuildOutput(file))
                .ToList();
            var tests = await TestCatalogScanner.ScanFilesAsync(sources, cancellationToken);

            if (tests.Count == 0)
            {
                selected.Add(assembly);
                continue;
            }

            var impacted = await analyzer.GetImpactedTestsAsync(changedFiles, tests, cancellationToken);
            if (impacted.Count > 0)
            {
                selected.Add(assembly);
            }
        }

        return selected.Count > 0 ? selected : assemblies;
    }

    private static string? FindProjectDirectory(string path)
    {
        if (path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetDirectoryName(Path.GetFullPath(path));
        }

        for (var directory = Path.GetDirectoryName(Path.GetFullPath(path));
             directory is not null;
             directory = Path.GetDirectoryName(directory))
        {
            if (Directory.EnumerateFiles(directory, "*.csproj").Any())
            {
                return directory;
            }
        }

        return null;
    }

    private static bool IsBuildOutput(string file)
    {
        var normalized = file.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }
}
