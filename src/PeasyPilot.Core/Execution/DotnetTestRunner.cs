using System.Diagnostics;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Execution;

/// <summary>
/// Runs a test project with <c>dotnet test</c> and reads the TRX report it writes.
/// Used for frameworks without a Microsoft Testing Platform executable (NUnit). A run that writes no report,
/// or several reports for an unspecified framework, is an error, never a success.
/// </summary>
public sealed class DotnetTestRunner
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(15);
    private const string LogFileName = "report.trx";

    /// <summary>
    /// Builds the <c>dotnet test</c> arguments that write one TRX report into <paramref name="resultsDirectory"/>.
    /// </summary>
    /// <param name="projectPath">The test project file.</param>
    /// <param name="resultsDirectory">Where the report is written.</param>
    /// <param name="framework">The target framework to run, or null when the project has one.</param>
    /// <returns>The arguments to pass to <c>dotnet</c>.</returns>
    public static IReadOnlyList<string> BuildArguments(string projectPath, string resultsDirectory, string? framework)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultsDirectory);

        var arguments = new List<string>
        {
            "test", projectPath,
            "--logger", $"trx;LogFileName={LogFileName}",
            "--results-directory", resultsDirectory
        };

        if (!string.IsNullOrWhiteSpace(framework))
        {
            arguments.Add("--framework");
            arguments.Add(framework);
        }

        return arguments;
    }

    /// <summary>
    /// Runs one test project and returns its results.
    /// </summary>
    /// <param name="projectPath">The test project file (.csproj).</param>
    /// <param name="framework">The target framework to run, required when the project targets several.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The run result, including each test outcome.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the project does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no report, or several reports, were written.</exception>
    public async Task<TestRunResult> RunAsync(string projectPath, string? framework = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var fullPath = Path.GetFullPath(projectPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Test project not found.", fullPath);
        }

        // Several frameworks write reports to the same file name, so the last one would silently win.
        if (string.IsNullOrWhiteSpace(framework) && TargetsSeveralFrameworks(await File.ReadAllTextAsync(fullPath, cancellationToken)))
        {
            throw new InvalidOperationException(
                $"'{fullPath}' targets several frameworks. Pass --framework to choose one.");
        }

        var resultsDirectory = Path.Combine(Path.GetTempPath(), "peasypilot", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(resultsDirectory);

        // A failing test makes dotnet test exit non-zero; that is a result, so only the report decides.
        await CaptureAsync(BuildArguments(fullPath, resultsDirectory, framework), cancellationToken);

        var reports = Directory.EnumerateFiles(resultsDirectory, "*.trx", SearchOption.AllDirectories).ToList();
        if (reports.Count == 0)
        {
            throw new InvalidOperationException($"'{fullPath}' wrote no TRX report (did the build fail?).");
        }

        if (reports.Count > 1)
        {
            throw new InvalidOperationException(
                $"'{fullPath}' targets several frameworks and wrote {reports.Count} reports. Pass --framework to choose one.");
        }

        var xml = await File.ReadAllTextAsync(reports[0], cancellationToken);
        return TrxReportParser.Parse(xml);
    }

    /// <summary>
    /// Tells whether a project file targets more than one framework (a <c>TargetFrameworks</c> list).
    /// </summary>
    /// <param name="projectXml">The project file content.</param>
    /// <returns>True when several frameworks are listed.</returns>
    public static bool TargetsSeveralFrameworks(string projectXml)
    {
        ArgumentNullException.ThrowIfNull(projectXml);

        var match = System.Text.RegularExpressions.Regex.Match(projectXml, "<TargetFrameworks>([^<]*)</TargetFrameworks>");
        return match.Success && match.Groups[1].Value.Contains(';');
    }

    private static async Task CaptureAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start dotnet.");

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(DefaultTimeout);

        var stdout = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"dotnet test did not finish within {DefaultTimeout}.");
        }

        await Task.WhenAll(stdout, stderr);
    }
}
