

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobTracker.Api.IntegrationTests.Authentication;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-Test-UserId"].FirstOrDefault() ?? "test-user-id";
        var authentication = Request.Headers["X-Test-Unauthenticated"].FirstOrDefault() ?? "true";

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId),
            new Claim(
                ClaimTypes.Name,
                "Test User")
        };

        var identity = new ClaimsIdentity(claims, Scheme);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, Scheme);

        //return Task.FromResult(AuthenticateResult.Success(ticket));
        return authentication == "true" ? Task.FromResult(AuthenticateResult.Success(ticket)) : Task.FromResult(AuthenticateResult.NoResult());
    }
}