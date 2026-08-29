using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PeasyPilot.Integration.Tests;

public class BasicIntegrationTests : IClassFixture<WebApplicationFactory<PeasyPilot.Integration.Startup>>
{
    private readonly WebApplicationFactory<PeasyPilot.Integration.Startup> _factory;

    public BasicIntegrationTests(WebApplicationFactory<PeasyPilot.Integration.Startup> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Test1_Passing()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.True(true); // Placeholder
    }
}