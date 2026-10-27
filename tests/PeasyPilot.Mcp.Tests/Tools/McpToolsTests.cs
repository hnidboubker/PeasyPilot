using PeasyPilot.Mcp.Tools;
using PeasyPilot.Core.Assertions;
using Xunit;

namespace PeasyPilot.Mcp.Tests.Tools;

public class McpToolsTests
{
    [Fact]
    public async Task AnalyzeFailureTool_DiagnosesNullReferenceException()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>
        {
            { "testName", "SampleTest" },
            { "errorMessage", "NullReferenceException: Object reference not set" }
        };

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.True(dynamicResult.success);
        PeasyAssert.Equal("NULL_REFERENCE", (string)dynamicResult.failureType);
        PeasyAssert.Equal("CRITICAL", (string)dynamicResult.riskLevel);
    }

    [Fact]
    public async Task AnalyzeFailureTool_DiagnosesAssertionFailure()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>
        {
            { "testName", "SampleTest" },
            { "errorMessage", "PeasyAssert.Equal failed: expected 10 but got 5" }
        };

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.True(dynamicResult.success);
        PeasyAssert.Equal("ASSERTION_FAILED", (string)dynamicResult.failureType);
        PeasyAssert.Equal("MEDIUM", (string)dynamicResult.riskLevel);
    }

    [Fact]
    public async Task AnalyzeFailureTool_DiagnosesTimeoutException()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>
        {
            { "testName", "SlowTest" },
            { "errorMessage", "Test timed out after 30000ms" }
        };

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.True(dynamicResult.success);
        PeasyAssert.Equal("TIMEOUT", (string)dynamicResult.failureType);
        PeasyAssert.Equal("HIGH", (string)dynamicResult.riskLevel);
    }

    [Fact]
    public async Task AnalyzeFailureTool_DiagnosesMockExpectationFailure()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>
        {
            { "testName", "MockTest" },
            { "errorMessage", "Expected mock was not called" }
        };

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.True(dynamicResult.success);
        PeasyAssert.Equal("MOCK_EXPECTATION", (string)dynamicResult.failureType);
    }

    [Fact]
    public async Task AnalyzeFailureTool_DiagnosesSetupError()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>
        {
            { "testName", "SetupTest" },
            { "errorMessage", "SetUp failed: database initialization error" }
        };

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.True(dynamicResult.success);
        PeasyAssert.Equal("SETUP_ERROR", (string)dynamicResult.failureType);
        PeasyAssert.Equal("HIGH", (string)dynamicResult.riskLevel);
    }

    [Fact]
    public async Task AnalyzeFailureTool_ReturnsErrorForMissingErrorMessage()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.False(dynamicResult.success);
    }

    [Fact]
    public async Task RunTestsTool_ReturnsErrorForMissingProjectPath()
    {
        var tool = new RunTestsTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.False(dynamicResult.success);
    }

    [Fact]
    public async Task ChallengeTestsTool_ReturnsErrorForMissingRequiredParameters()
    {
        var tool = new ChallengeTestsTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.False(dynamicResult.success);
    }

    [Fact]
    public async Task GenerateTestsTool_ReturnsErrorForMissingTypeName()
    {
        var tool = new GenerateTestsTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        PeasyAssert.False(dynamicResult.success);
    }
}
