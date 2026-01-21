using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Budget.Api.Server;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class FakeJwtOptions : AuthenticationSchemeOptions
{
}

public class FakeAuthHandler(IOptionsMonitor<FakeJwtOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<FakeJwtOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        var token = authHeader.ToString().Replace("Bearer ", "");

        try
        {
            var decodedBytes = Convert.FromBase64String(token);
            var decodedString = System.Text.Encoding.UTF8.GetString(decodedBytes);
            var username = decodedString.Replace("fake-jwt-for-", "");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("scope", "api_access")
            };

            var identity = new ClaimsIdentity(claims, "FakeJwt");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "FakeJwt");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Fake Token"));
        }
    }
}