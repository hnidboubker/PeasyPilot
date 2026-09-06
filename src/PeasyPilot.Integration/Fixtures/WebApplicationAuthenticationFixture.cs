using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PeasyPilot.Integration.Helpers;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// Base fixture for testing ASP.NET Core applications with authentication enabled.
/// Provides utilities for setting up test authentication schemes.
/// </summary>
public abstract class WebApplicationAuthenticationFixture<TStartup> : IAsyncDisposable
    where TStartup : class
{
    private WebApplicationTestFactory<TStartup>? _factory;

    /// <summary>
    /// Gets the application test factory.
    /// </summary>
    protected WebApplicationTestFactory<TStartup> Factory =>
        _factory ?? throw new InvalidOperationException("Factory not initialized. Call InitializeAsync first.");

    /// <summary>
    /// Gets an HTTP client for making authenticated requests.
    /// </summary>
    protected HttpTestClient TestClient => Factory.CreateHttpTestClient();

    /// <summary>
    /// Configures authentication for the test application.
    /// Override this to register test authentication schemes.
    /// </summary>
    protected virtual void ConfigureAuthentication(AuthenticationBuilder authBuilder)
    {
        // Default: add a test scheme that accepts any token
        authBuilder.AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(
            "TestScheme",
            opts => opts.UserId = "test-user");
    }

    /// <summary>
    /// Initializes the application factory with authentication configured.
    /// </summary>
    public virtual Task InitializeAsync()
    {
        _factory = new WebApplicationTestFactory<TStartup>()
            .WithServices(services =>
            {
                services.AddAuthentication()
                    .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>(
                        "TestScheme",
                        opts => opts.UserId = "test-user");
            });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the application factory and releases resources.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        if (_factory is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _factory?.Dispose();
        }
    }

    /// <summary>
    /// Creates an HTTP client with a specific user ID for testing authorization.
    /// </summary>
    protected HttpTestClient CreateAuthenticatedClient(string userId)
    {
        var client = Factory.CreateHttpTestClient();
        client.Client.DefaultRequestHeaders.Add("X-Test-User", userId);
        return client;
    }
}
