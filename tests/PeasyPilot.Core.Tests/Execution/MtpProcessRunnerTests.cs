using PeasyPilot.Core.Execution;
using PeasyPilot.XUnit;
using Xunit;

namespace PeasyPilot.Core.Tests.Execution;

public class MtpProcessRunnerTests
{
    [Fact]
    public void ReportArguments_XUnitHelp_UsesResultTrx()
    {
        const string help = "  -result-trx <filename> : output results to TRX (XML) file";

        var args = MtpProcessRunner.ReportArguments(help, "report.trx");

        XAssert.Equal(new[] { "-result-trx", "report.trx" }, args);
    }

    [Fact]
    public void ReportArguments_TUnitHelp_UsesReportTrxWithFilename()
    {
        const string help = "    --report-trx\n        Activer la génération du rapport TRX.";

        var args = MtpProcessRunner.ReportArguments(help, "report.trx");

        XAssert.Equal(new[] { "--report-trx", "--report-trx-filename", "report.trx" }, args);
    }

    [Fact]
    public void ReportArguments_NoTrxOption_Throws()
    {
        XAssert.Throws<InvalidOperationException>(() => MtpProcessRunner.ReportArguments("usage: nothing", "report.trx"));
    }

    [Fact]
    public async Task RunAsync_MissingExecutable_ThrowsFileNotFound()
    {
        var runner = new MtpProcessRunner();

        await XAssert.ThrowsAsync<FileNotFoundException>(() => runner.RunAsync("does-not-exist.exe"));
    }
}
