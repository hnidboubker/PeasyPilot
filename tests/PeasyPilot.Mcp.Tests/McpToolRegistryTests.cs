using PeasyPilot.Mcp.Services;
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

        Assert.NotEmpty(allTools);
        Assert.Contains(allTools, t => t.Name == "analyze_code");
        Assert.Contains(allTools, t => t.Name == "generate_tests");
        Assert.Contains(allTools, t => t.Name == "run_tests");
        Assert.Contains(allTools, t => t.Name == "challenge_tests");
        Assert.Contains(allTools, t => t.Name == "analyze_failure");
        Assert.Contains(allTools, t => t.Name == "generate_and_run_tests");
    }

    [Fact]
    public void GetTool_ReturnsCorrectTool()
    {
        var registry = new McpToolRegistry();
        registry.RegisterDefaults();

        var analyzeTool = registry.GetTool("analyze_code");

        Assert.NotNull(analyzeTool);
        Assert.Equal("analyze_code", analyzeTool.Name);
    }

    [Fact]
    public void GetTool_ReturnsNullForUnknownTool()
    {
        var registry = new McpToolRegistry();
        registry.RegisterDefaults();

        var tool = registry.GetTool("unknown_tool");

        Assert.Null(tool);
    }

    [Fact]
    public void Register_CustomTool_RegistersSuccessfully()
    {
        var registry = new McpToolRegistry();
        var customTool = new TestTool();

        registry.Register(customTool);
        var retrieved = registry.GetTool("test_tool");

        Assert.NotNull(retrieved);
        Assert.Same(customTool, retrieved);
    }

    private class TestTool : IMcpTool
    {
        public string Name => "test_tool";
        public string Description => "Test tool";

        public Task<object> ExecuteAsync(Dictionary<string, object> parameters) =>
            Task.FromResult((object)new { success = true });
    }
}
