using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Budget.Api.Server;

public class FakeAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "FakeAuth";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var username = Request.Cookies["FakeAuth.Username"];

        if (string.IsNullOrEmpty(username))
        {
            Response.Redirect("/fake-login");
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        // Create a dummy user
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Dev")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        ticket.Properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = "fake-token" }]);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}