using PeasyPilot.Core.Execution;
using PeasyPilot.Core.Assertions;
using Xunit;

namespace PeasyPilot.Core.Tests.Execution;

public class MtpProcessRunnerTests
{
    [Fact]
    public void ReportArguments_XUnitHelp_UsesResultTrx()
    {
        const string help = "  -result-trx <filename> : output results to TRX (XML) file";

        var args = MtpProcessRunner.ReportArguments(help, "report.trx");

        PeasyAssert.Equal(new[] { "-result-trx", "report.trx" }, args);
    }

    [Fact]
    public void ReportArguments_TUnitHelp_UsesReportTrxWithFilename()
    {
        const string help = "    --report-trx\n        Activer la génération du rapport TRX.";

        var args = MtpProcessRunner.ReportArguments(help, "report.trx");

        PeasyAssert.Equal(new[] { "--report-trx", "--report-trx-filename", "report.trx" }, args);
    }

    [Fact]
    public void ReportArguments_NoTrxOption_Throws()
    {
        PeasyAssert.Throws<InvalidOperationException>(() => MtpProcessRunner.ReportArguments("usage: nothing", "report.trx"));
    }

    [Fact]
    public async Task RunAsync_MissingExecutable_ThrowsFileNotFound()
    {
        var runner = new MtpProcessRunner();

        await PeasyAssert.ThrowsAsync<FileNotFoundException>(() => runner.RunAsync("does-not-exist.exe"));
    }
}
