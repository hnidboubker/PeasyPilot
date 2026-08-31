using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// WebApplicationFactory extension that supports overriding and intercepting DI service registrations for integration tests.
/// </summary>
/// <typeparam name="TStartup">The startup/program class of the Web application.</typeparam>
public class InterceptedWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly Action<IServiceCollection>? _configureServices;

    /// <summary>
    /// Initializes a new instance of the <see cref="InterceptedWebApplicationFactory{TStartup}"/> class.
    /// </summary>
    /// <param name="configureServices">Action to configure or override DI services for testing.</param>
    public InterceptedWebApplicationFactory(Action<IServiceCollection>? configureServices = null)
    {
        _configureServices = configureServices;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            _configureServices?.Invoke(services);
        });
    }
}
