using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace PeasyPilot.Integration.Tests;

public class BasicIntegrationTests
{
    [Fact]
    public async Task Test1_Passing()
    {
        // Arrange
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseStartup<Startup>();
            });

        using var host = await hostBuilder.StartAsync();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}