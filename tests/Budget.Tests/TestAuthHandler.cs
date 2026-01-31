using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Budget.Tests;

public class TestAuthOptions : AuthenticationSchemeOptions
{
    public string UserName { get; set; } = "Test user";
}

public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
#pragma warning disable CS0618 // Type or member is obsolete
        ISystemClock clock)
        : base(options, logger, encoder, clock)
#pragma warning restore CS0618 // Type or member is obsolete
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] {
            new Claim(ClaimTypes.Name, Options.UserName),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
