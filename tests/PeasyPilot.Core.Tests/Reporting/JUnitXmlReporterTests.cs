using System.Xml.Linq;
using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Models;
using PeasyPilot.Core.Reporting;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.Reporting;

public class JUnitXmlReporterTests
{
    [Fact]
    public async Task ReportAsync_WithTests_EmitsOneTestcasePerTest()
    {
        var run = new TestRunResult
        {
            Passed = 1,
            Failed = 1,
            Skipped = 1,
            Status = TestRunStatus.Failed,
            Tests =
            [
                new() { Name = "Shop.Orders.Adds", Status = TestRunStatus.Passed },
                new() { Name = "Shop.Orders.Divides", Status = TestRunStatus.Failed, Message = "boom" },
                new() { Name = "Shop.Orders.Pending", Status = TestRunStatus.Skipped }
            ]
        };

        var xml = await new JUnitXmlReporter().ReportAsync(run);
        var testcases = XDocument.Parse(xml).Descendants("testcase").ToList();

        XAssert.Equal(3, testcases.Count);
        XAssert.Equal("Shop.Orders", testcases[0].Attribute("classname")!.Value);
        XAssert.Equal("boom", testcases[1].Element("failure")!.Attribute("message")!.Value);
        XAssert.NotNull(testcases[2].Element("skipped"));
    }

    [Fact]
    public async Task ReportAsync_WithoutTests_EmitsNoTestcase()
    {
        var run = new TestRunResult { Passed = 2, Status = TestRunStatus.Passed };

        var xml = await new JUnitXmlReporter().ReportAsync(run);

        XAssert.IsEmpty(XDocument.Parse(xml).Descendants("testcase"));
    }
}
