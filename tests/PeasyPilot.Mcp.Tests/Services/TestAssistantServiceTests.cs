using PeasyPilot.Mcp.Services;
using PeasyPilot.Core.Assertions;
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

        PeasyAssert.True(result.Success);
        PeasyAssert.NotNull(result.Result);
    }

    [Fact]
    public async Task ExecuteToolAsync_ReturnsErrorForUnknownTool()
    {
        var registry = new McpToolRegistry();
        var service = new TestAssistantService(registry);

        var result = await service.ExecuteToolAsync("unknown_tool", new Dictionary<string, object>());

        PeasyAssert.False(result.Success);
        PeasyAssert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteToolAsync_ReturnsExecutionTime()
    {
        var registry = new McpToolRegistry();
        var mockTool = new MockTool();
        registry.Register(mockTool);

        var service = new TestAssistantService(registry);
        var result = await service.ExecuteToolAsync("mock_tool", new Dictionary<string, object>());

        PeasyAssert.True(result.ExecutionTimeMs >= 0);
    }

    [Fact]
    public void ServiceConstructor_ThrowsWhenRegistryIsNull()
    {
        PeasyAssert.Throws<ArgumentNullException>(() => new TestAssistantService(null!));
    }

    private class MockTool : IMcpTool
    {
        public string Name => "mock_tool";
        public string Description => "Mock tool for testing";

        public Task<object> ExecuteAsync(Dictionary<string, object> parameters) =>
            Task.FromResult((object)new { success = true });
    }
}
