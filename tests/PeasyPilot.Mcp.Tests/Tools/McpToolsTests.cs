using PeasyPilot.Mcp.Tools;
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

        Assert.True(dynamicResult.success);
        Assert.Equal("NULL_REFERENCE", (string)dynamicResult.failureType);
        Assert.Equal("CRITICAL", (string)dynamicResult.riskLevel);
    }

    [Fact]
    public async Task AnalyzeFailureTool_DiagnosesAssertionFailure()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>
        {
            { "testName", "SampleTest" },
            { "errorMessage", "Assert.Equal failed: expected 10 but got 5" }
        };

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        Assert.True(dynamicResult.success);
        Assert.Equal("ASSERTION_FAILED", (string)dynamicResult.failureType);
        Assert.Equal("MEDIUM", (string)dynamicResult.riskLevel);
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

        Assert.True(dynamicResult.success);
        Assert.Equal("TIMEOUT", (string)dynamicResult.failureType);
        Assert.Equal("HIGH", (string)dynamicResult.riskLevel);
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

        Assert.True(dynamicResult.success);
        Assert.Equal("MOCK_EXPECTATION", (string)dynamicResult.failureType);
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

        Assert.True(dynamicResult.success);
        Assert.Equal("SETUP_ERROR", (string)dynamicResult.failureType);
        Assert.Equal("HIGH", (string)dynamicResult.riskLevel);
    }

    [Fact]
    public async Task AnalyzeFailureTool_ReturnsErrorForMissingErrorMessage()
    {
        var tool = new AnalyzeFailureTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        Assert.False(dynamicResult.success);
    }

    [Fact]
    public async Task RunTestsTool_ReturnsErrorForMissingProjectPath()
    {
        var tool = new RunTestsTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        Assert.False(dynamicResult.success);
    }

    [Fact]
    public async Task ChallengeTestsTool_ReturnsErrorForMissingRequiredParameters()
    {
        var tool = new ChallengeTestsTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        Assert.False(dynamicResult.success);
    }

    [Fact]
    public async Task GenerateTestsTool_ReturnsErrorForMissingTypeName()
    {
        var tool = new GenerateTestsTool();
        var parameters = new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(parameters);
        dynamic dynamicResult = result;

        Assert.False(dynamicResult.success);
    }
}
