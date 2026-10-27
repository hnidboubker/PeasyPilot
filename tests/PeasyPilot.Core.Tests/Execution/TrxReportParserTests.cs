using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Execution;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.Execution;

public class TrxReportParserTests
{
    private const string MixedRun = """
        <?xml version="1.0" encoding="utf-8"?>
        <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
          <Results>
            <UnitTestResult testName="Adds" outcome="Passed" duration="00:00:00.0100000" />
            <UnitTestResult testName="Divides" outcome="Failed" duration="00:00:00.0200000">
              <Output><ErrorInfo><Message>Expected 2 but was 3</Message></ErrorInfo></Output>
            </UnitTestResult>
            <UnitTestResult testName="Pending" outcome="NotExecuted" duration="00:00:00" />
          </Results>
        </TestRun>
        """;

    [Fact]
    public void Parse_MixedRun_CountsEachOutcome()
    {
        var (summary, _) = TrxReportParser.Parse(MixedRun);

        XAssert.Equal(1, summary.Passed);
        XAssert.Equal(1, summary.Failed);
        XAssert.Equal(1, summary.Skipped);
    }

    [Fact]
    public void Parse_MixedRun_IsFailedAndSumsDurations()
    {
        var (summary, _) = TrxReportParser.Parse(MixedRun);

        XAssert.Equal(TestRunStatus.Failed, summary.Status);
        XAssert.Equal(TimeSpan.FromMilliseconds(30), summary.Duration);
    }

    [Fact]
    public void Parse_MixedRun_KeepsNamesAndFailureMessage()
    {
        var (_, tests) = TrxReportParser.Parse(MixedRun);

        var failed = tests.Single(t => t.Name == "Divides");
        XAssert.Equal(TestRunStatus.Failed, failed.Status);
        XAssert.Equal("Expected 2 but was 3", failed.Message);
    }

    [Fact]
    public void Parse_EmptyRun_IsNotStarted()
    {
        const string empty = """
            <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results /></TestRun>
            """;

        var (summary, tests) = TrxReportParser.Parse(empty);

        XAssert.Equal(TestRunStatus.NotStarted, summary.Status);
        XAssert.IsEmpty(tests);
    }
}
