using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeasyPilot.Integration.Helpers;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// Test factory for ASP.NET Core applications with configurable services and middleware.
/// Enables easy setup of test environments with database overrides and service mocking.
/// </summary>
/// <typeparam name="TStartup">The startup class of the application under test.</typeparam>
public class WebApplicationTestFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private Action<IServiceCollection>? _configureServices;
    private Action<IApplicationBuilder>? _configureApp;
    private Action<IWebHostBuilder>? _configureWebHost;

    /// <summary>
    /// Configures services for the test application.
    /// Use this to override or mock dependencies.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithServices(
        Action<IServiceCollection> configure)
    {
        _configureServices = configure;
        return this;
    }

    /// <summary>
    /// Configures the application middleware pipeline.
    /// Use this to add or replace middleware for testing.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithApp(
        Action<IApplicationBuilder> configure)
    {
        _configureApp = configure;
        return this;
    }

    /// <summary>
    /// Configures the web host builder directly.
    /// Use this for advanced scenarios.
    /// </summary>
    public WebApplicationTestFactory<TStartup> WithWebHostBuilder(
        Action<IWebHostBuilder> configure)
    {
        _configureWebHost = configure;
        return this;
    }

    /// <summary>
    /// Creates an HTTP client for making requests to the test application.
    /// </summary>
    public HttpClient CreateTestClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Creates an HTTP client with automatic response assertion helpers.
    /// </summary>
    public HttpTestClient CreateHttpTestClient()
    {
        return new HttpTestClient(CreateTestClient());
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseStartup<TStartup>()
            .ConfigureServices(_configureServices ?? (s => { }))
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            });

        _configureWebHost?.Invoke(builder);
    }

    /// <inheritdoc />
    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
                _configureWebHost?.Invoke(webBuilder);
            });
    }
}
