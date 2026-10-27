using PeasyPilot.Core.Eums;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Execution;
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
        var summary = TrxReportParser.Parse(MixedRun);

        PeasyAssert.Equal(1, summary.Passed);
        PeasyAssert.Equal(1, summary.Failed);
        PeasyAssert.Equal(1, summary.Skipped);
    }

    [Fact]
    public void Parse_MixedRun_IsFailedAndSumsDurations()
    {
        var summary = TrxReportParser.Parse(MixedRun);

        PeasyAssert.Equal(TestRunStatus.Failed, summary.Status);
        PeasyAssert.Equal(TimeSpan.FromMilliseconds(30), summary.Duration);
    }

    [Fact]
    public void Parse_MixedRun_KeepsNamesAndFailureMessage()
    {
        var tests = TrxReportParser.Parse(MixedRun).Tests;

        var failed = tests.Single(t => t.Name == "Divides");
        PeasyAssert.Equal(TestRunStatus.Failed, failed.Status);
        PeasyAssert.Equal("Expected 2 but was 3", failed.Message);
    }

    [Fact]
    public void Parse_EmptyRun_IsNotStarted()
    {
        const string empty = """
            <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results /></TestRun>
            """;

        var summary = TrxReportParser.Parse(empty);
        var tests = summary.Tests;

        PeasyAssert.Equal(TestRunStatus.NotStarted, summary.Status);
        PeasyAssert.IsEmpty(tests);
    }
}
