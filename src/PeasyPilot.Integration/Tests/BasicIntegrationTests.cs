using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace PeasyPilot.Integration.Tests;

public class BasicIntegrationTests
{
    [Fact]
    public async Task Test1_Passing()
    {
        // Arrange
        var builder = new WebHostBuilder()
            .UseStartup<Startup>()
            .UseTestServer();

        await using var server = await builder.StartAsync();
        var client = server.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}