using System.Globalization;
using System.Xml.Linq;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Execution;

/// <summary>
/// Reads a TRX report written by a test executable (xUnit v3 or TUnit in Microsoft Testing Platform mode).
/// </summary>
public static class TrxReportParser
{
    private static readonly XNamespace Ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    /// <summary>
    /// Parses a TRX document into a run result, with one entry per executed test.
    /// </summary>
    /// <param name="trxXml">The TRX document content.</param>
    /// <returns>The run result, including its per-test results.</returns>
    public static TestRunResult Parse(string trxXml)
    {
        ArgumentNullException.ThrowIfNull(trxXml);

        var tests = XDocument.Parse(trxXml)
            .Descendants(Ns + "UnitTestResult")
            .Select(ToTestResult)
            .ToList();

        return new TestRunResult
        {
            Passed = tests.Count(t => t.Status == TestRunStatus.Passed),
            Failed = tests.Count(t => t.Status == TestRunStatus.Failed),
            Skipped = tests.Count(t => t.Status == TestRunStatus.Skipped),
            Duration = tests.Aggregate(TimeSpan.Zero, (total, t) => total + t.Duration),
            Status = tests.Count == 0
                ? TestRunStatus.NotStarted
                : tests.Any(t => t.Status == TestRunStatus.Failed)
                    ? TestRunStatus.Failed
                    : TestRunStatus.Passed,
            Tests = tests
        };
    }

    private static TestResult ToTestResult(XElement element)
    {
        var outcome = (string?)element.Attribute("outcome") ?? string.Empty;
        var status = outcome switch
        {
            "Passed" => TestRunStatus.Passed,
            "Failed" or "Error" or "Timeout" or "Aborted" => TestRunStatus.Failed,
            _ => TestRunStatus.Skipped
        };

        var duration = TimeSpan.TryParse((string?)element.Attribute("duration"), CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : TimeSpan.Zero;

        return new TestResult
        {
            Name = (string?)element.Attribute("testName") ?? string.Empty,
            Status = status,
            Duration = duration,
            Message = element.Descendants(Ns + "Message").FirstOrDefault()?.Value
        };
    }
}
