using PeasyPilot.Mcp.Services;
using PeasyPilot.Mcp.Tools;
using Xunit;

namespace PeasyPilot.Mcp.Tests.Services;

public class TestAssistantServiceTests
{
    [Fact]
    public async Task ExecuteToolAsync_CallsRegisteredTool()
    {
        var registry = new McpToolRegistry();
        var mockTool = new MockTool();
        registry.Register(mockTool);

        var service = new TestAssistantService(registry);
        var result = await service.ExecuteToolAsync("mock_tool", new Dictionary<string, object>());

        Assert.True(result.Success);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task ExecuteToolAsync_ReturnsErrorForUnknownTool()
    {
        var registry = new McpToolRegistry();
        var service = new TestAssistantService(registry);

        var result = await service.ExecuteToolAsync("unknown_tool", new Dictionary<string, object>());

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteToolAsync_ReturnsExecutionTime()
    {
        var registry = new McpToolRegistry();
        var mockTool = new MockTool();
        registry.Register(mockTool);

        var service = new TestAssistantService(registry);
        var result = await service.ExecuteToolAsync("mock_tool", new Dictionary<string, object>());

        Assert.True(result.ExecutionTimeMs >= 0);
    }

    [Fact]
    public void ServiceConstructor_ThrowsWhenRegistryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TestAssistantService(null!));
    }

    private class MockTool : IMcpTool
    {
        public string Name => "mock_tool";
        public string Description => "Mock tool for testing";

        public Task<object> ExecuteAsync(Dictionary<string, object> parameters) =>
            Task.FromResult((object)new { success = true });
    }
}
