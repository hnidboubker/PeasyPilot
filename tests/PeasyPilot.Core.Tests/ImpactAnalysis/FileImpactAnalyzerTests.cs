using PeasyPilot.Core.ImpactAnalysis;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Core.Models;
using Xunit;

namespace PeasyPilot.Core.Tests.ImpactAnalysis;

public class FileImpactAnalyzerTests
{
    private static TestCase TestIn(string name, string? file) =>
        file is null
            ? new TestCase { Name = name }
            : new TestCase { Name = name, Metadata = new() { ["File"] = file } };

    [Fact]
    public async Task GetImpactedTests_NoChangedFiles_ReturnsEveryTest()
    {
        var tests = new[] { TestIn("A", "/repo/tests/OrderServiceTests.cs"), TestIn("B", "/repo/tests/PaymentTests.cs") };

        var impacted = await new FileImpactAnalyzer().GetImpactedTestsAsync([], tests);

        PeasyAssert.Equal(2, impacted.Count);
    }

    [Fact]
    public async Task GetImpactedTests_TestWithoutKnownFile_IsAlwaysSelected()
    {
        var tests = new[] { TestIn("Unknown", null), TestIn("Other", "/repo/tests/PaymentTests.cs") };

        var impacted = await new FileImpactAnalyzer().GetImpactedTestsAsync(["src/Invoice.cs"], tests);

        PeasyAssert.Equal("Unknown", impacted.Single().Name);
    }

    [Fact]
    public async Task GetImpactedTests_ChangedTestFile_SelectsItsTests()
    {
        var tests = new[] { TestIn("Order", "/repo/tests/Shop/OrderServiceTests.cs"), TestIn("Payment", "/repo/tests/PaymentTests.cs") };

        var impacted = await new FileImpactAnalyzer().GetImpactedTestsAsync(["tests/Shop/OrderServiceTests.cs"], tests);

        PeasyAssert.Equal("Order", impacted.Single().Name);
    }

    [Fact]
    public async Task GetImpactedTests_ChangedProductionFile_SelectsTestFileNamedAfterIt()
    {
        var tests = new[] { TestIn("Order", "/repo/tests/OrderServiceTests.cs"), TestIn("Payment", "/repo/tests/PaymentTests.cs") };

        var impacted = await new FileImpactAnalyzer().GetImpactedTestsAsync(["src\\Shop\\OrderService.cs"], tests);

        PeasyAssert.Equal("Order", impacted.Single().Name);
    }

    [Fact]
    public void Parse_GitOutput_SplitsLinesAndNormalizesSeparators()
    {
        var paths = GitChangedFiles.Parse("src/A.cs\r\nsrc\\B.cs\n\n");

        PeasyAssert.Equal(2, paths.Count);
        PeasyAssert.Equal("src/B.cs", paths[1]);
    }
}
