using System.Text;
using System.Xml.Linq;
using PeasyPilot.Core.Abstractions;
using PeasyPilot.Core.Models;

namespace PeasyPilot.Core.Reporting;

/// <summary>
/// Formats test run results into JUnit XML format for CI/CD pipeline consumption.
/// </summary>
public sealed class JUnitXmlReporter : ITestReporter
{
    private readonly string? _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="JUnitXmlReporter"/> class.
    /// </summary>
    /// <param name="filePath">Optional file path to save the XML output.</param>
    public JUnitXmlReporter(string? filePath = null)
    {
        _filePath = filePath;
    }

    /// <inheritdoc />
    public async Task<string> ReportAsync(TestRunResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        var total = result.Passed + result.Failed + result.Skipped;
        var durationSeconds = result.Duration.TotalSeconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);

        var xmlElement = new XElement("testsuites",
            new XElement("testsuite",
                new XAttribute("name", "PeasyPilot Test Suite"),
                new XAttribute("tests", total),
                new XAttribute("failures", result.Failed),
                new XAttribute("skipped", result.Skipped),
                new XAttribute("time", durationSeconds),
                new XAttribute("timestamp", DateTime.UtcNow.ToString("o"))
            )
        );

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), xmlElement);
        var xmlString = doc.ToString();

        if (!string.IsNullOrWhiteSpace(_filePath))
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_filePath, xmlString, cancellationToken);
        }

        return xmlString;
    }
}
