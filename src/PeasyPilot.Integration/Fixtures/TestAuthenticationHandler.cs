using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PeasyPilot.Integration.Fixtures;

/// <summary>
/// Test authentication options for configuring the test authentication scheme.
/// </summary>
public class TestAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Gets or sets the default user ID for authenticated requests.
    /// </summary>
    public string UserId { get; set; } = "test-user";
}

/// <summary>
/// Test authentication handler that accepts any request and creates a test principal.
/// Used for testing authorization scenarios without a real authentication provider.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAuthenticationHandler"/> class.
    /// </summary>
    public TestAuthenticationHandler(
        IOptionsMonitor<TestAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Options.UserId;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId),
            new Claim("user_id", userId)
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
