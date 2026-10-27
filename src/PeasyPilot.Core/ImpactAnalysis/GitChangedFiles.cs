using System.Diagnostics;

namespace PeasyPilot.Core.ImpactAnalysis;

/// <summary>
/// Lists the files changed in a Git repository since a given reference, using <c>git diff --name-only</c>.
/// </summary>
public static class GitChangedFiles
{
    /// <summary>
    /// Returns the repository-relative paths changed between <paramref name="baseRef"/> and the working tree.
    /// </summary>
    /// <param name="repositoryPath">The repository root.</param>
    /// <param name="baseRef">The reference to compare against, such as <c>main</c> or <c>HEAD~1</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Changed paths using forward slashes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when git cannot run or the reference is invalid.</exception>
    public static async Task<IReadOnlyList<string>> ListSinceAsync(
        string repositoryPath,
        string baseRef,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseRef);

        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = repositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("diff");
        startInfo.ArgumentList.Add("--name-only");
        startInfo.ArgumentList.Add(baseRef);
        startInfo.ArgumentList.Add("--");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start git.");

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"git diff failed for '{baseRef}': {stderr.Trim()}");
        }

        return Parse(stdout);
    }

    /// <summary>
    /// Parses the output of <c>git diff --name-only</c>.
    /// </summary>
    /// <param name="gitOutput">The raw output, one path per line.</param>
    /// <returns>Non-empty paths using forward slashes.</returns>
    public static IReadOnlyList<string> Parse(string gitOutput)
    {
        ArgumentNullException.ThrowIfNull(gitOutput);

        return gitOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(path => path.Replace('\\', '/'))
            .ToArray();
    }
}
