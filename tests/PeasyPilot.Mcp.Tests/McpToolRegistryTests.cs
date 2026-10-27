using PeasyPilot.Mcp.Services;
using PeasyPilot.Core.Assertions;
using PeasyPilot.Mcp.Tools;
using Xunit;

namespace PeasyPilot.Mcp.Tests;

public class McpToolRegistryTests
{
    [Fact]
    public void RegisterDefaults_RegistersAllSixTools()
    {
        var registry = new McpToolRegistry();
        registry.RegisterDefaults();

        var allTools = registry.GetAllTools().ToList();

        PeasyAssert.NotEmpty(allTools);
        PeasyAssert.Contains(allTools, t => t.Name == "analyze_code");
        PeasyAssert.Contains(allTools, t => t.Name == "generate_tests");
        PeasyAssert.Contains(allTools, t => t.Name == "run_tests");
        PeasyAssert.Contains(allTools, t => t.Name == "challenge_tests");
        PeasyAssert.Contains(allTools, t => t.Name == "analyze_failure");
        PeasyAssert.Contains(allTools, t => t.Name == "generate_and_run_tests");
    }

    [Fact]
    public void GetTool_ReturnsCorrectTool()
    {
        var registry = new McpToolRegistry();
        registry.RegisterDefaults();

        var analyzeTool = registry.GetTool("analyze_code");

        PeasyAssert.NotNull(analyzeTool);
        PeasyAssert.Equal("analyze_code", analyzeTool.Name);
    }

    [Fact]
    public void GetTool_ReturnsNullForUnknownTool()
    {
        var registry = new McpToolRegistry();
        registry.RegisterDefaults();

        var tool = registry.GetTool("unknown_tool");

        PeasyAssert.Null(tool);
    }

    [Fact]
    public void Register_CustomTool_RegistersSuccessfully()
    {
        var registry = new McpToolRegistry();
        var customTool = new TestTool();

        registry.Register(customTool);
        var retrieved = registry.GetTool("test_tool");

        PeasyAssert.NotNull(retrieved);
        PeasyAssert.Same(customTool, retrieved);
    }

    private class TestTool : IMcpTool
    {
        public string Name => "test_tool";
        public string Description => "Test tool";

        public Task<object> ExecuteAsync(Dictionary<string, object> parameters) =>
            Task.FromResult((object)new { success = true });
    }
}
