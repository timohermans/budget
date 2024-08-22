using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Budget.App.Server;

public static class AuthenticationExtensions
{
    public static string SchemeNameOidc = "oidc";

    /// <summary>
    /// Configures OpenIDConnect authentication. This does not add '.AddAuthorization' to services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config">AppSettings needs to contain a key "Auth", which binds all the keys in there. Originally tested with Keycloak and keys: ClientId, Authority, ClientSecret</param>
    /// <returns>The same service collection</returns>
    public static IServiceCollection AddOidcAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(SchemeNameOidc)
        .AddOpenIdConnect(SchemeNameOidc, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.PreferredUsername;
            config.GetSection("Auth").Bind(options);
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        return services;
    }

    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("");

        group.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
            .AllowAnonymous();

        // Sign out of the Cookie and OIDC handlers. If you do not sign out with the OIDC handler,
        // the user will automatically be signed back in the next time they visit a page that requires authentication
        // without being able to choose another account.
        group.MapPost("/logout", ([FromForm] string? returnUrl) => TypedResults.SignOut(GetAuthProperties(returnUrl),
            [CookieAuthenticationDefaults.AuthenticationScheme]));

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        // TODO: Use HttpContext.Request.PathBase instead.
        const string pathBase = "/";

        // Prevent open redirects.
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}