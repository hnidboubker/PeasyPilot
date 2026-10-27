using System.Diagnostics;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Execution;

/// <summary>
/// Runs a test executable built for Microsoft Testing Platform (xUnit v3 or TUnit) and reads the TRX report it writes.
/// A run that produces no report is an error, never a success.
/// </summary>
public sealed class MtpProcessRunner
{
    private readonly TimeSpan _timeout;

    /// <summary>
    /// Initializes a new runner.
    /// </summary>
    /// <param name="timeout">Maximum time for one executable. Defaults to ten minutes.</param>
    public MtpProcessRunner(TimeSpan? timeout = null)
    {
        _timeout = timeout ?? TimeSpan.FromMinutes(10);
    }

    /// <summary>
    /// Runs one test executable and returns its results.
    /// </summary>
    /// <param name="executablePath">Path to the test executable (.exe).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The run result, including each test outcome.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the executable does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the executable has no TRX option or writes no report.</exception>
    public async Task<TestRunResult> RunAsync(string executablePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);

        var fullPath = Path.GetFullPath(executablePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Test executable not found.", fullPath);
        }

        var help = await CaptureAsync(fullPath, ["--help"], cancellationToken);

        var reportDirectory = Path.Combine(Path.GetTempPath(), "peasypilot", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(reportDirectory);
        var reportPath = Path.Combine(reportDirectory, "report.trx");

        var arguments = ReportArguments(help.StandardOutput + help.StandardError, reportPath);
        await CaptureAsync(fullPath, arguments, cancellationToken);

        if (!File.Exists(reportPath))
        {
            throw new InvalidOperationException($"'{fullPath}' finished without writing a TRX report.");
        }

        var xml = await File.ReadAllTextAsync(reportPath, cancellationToken);
        return TrxReportParser.Parse(xml);
    }

    /// <summary>
    /// Chooses the command-line arguments that make a test executable write a TRX report.
    /// </summary>
    /// <param name="helpText">The executable's help output.</param>
    /// <param name="reportPath">Where the report must be written.</param>
    /// <returns>The arguments to pass.</returns>
    public static IReadOnlyList<string> ReportArguments(string helpText, string reportPath)
    {
        ArgumentNullException.ThrowIfNull(helpText);
        ArgumentException.ThrowIfNullOrWhiteSpace(reportPath);

        if (helpText.Contains("-result-trx", StringComparison.Ordinal))
        {
            return ["-result-trx", reportPath];
        }

        if (helpText.Contains("--report-trx", StringComparison.Ordinal))
        {
            return ["--report-trx", "--report-trx-filename", reportPath];
        }

        throw new InvalidOperationException(
            "The executable has no TRX report option. Expected an xUnit v3 or TUnit test executable.");
    }

    private async Task<ProcessOutput> CaptureAsync(
        string executablePath,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(executablePath)
        {
            WorkingDirectory = Path.GetDirectoryName(executablePath) ?? Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Could not start '{executablePath}'.");

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(_timeout);

        var stdout = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeoutSource.Token);

        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"'{executablePath}' did not finish within {_timeout}.");
        }

        return new ProcessOutput(process.ExitCode, await stdout, await stderr);
    }

    private sealed record ProcessOutput(int ExitCode, string StandardOutput, string StandardError);
}
