using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Budget.Ui.Server;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Configures OpenIDConnect authentication. This does not add '.AddAuthorization' to services
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config">AppSettings needs to contain a key "Auth", which binds all the keys in there. Originally tested with Keycloak and keys: ClientId, Authority, ClientSecret</param>
    /// <returns>The same service collection</returns>
    public static IServiceCollection AddOidcAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o => { o.SlidingExpiration = false; })
            .AddOpenIdConnect(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                config.GetSection("Auth").Bind(options);
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = true;
                options.UseTokenLifetime = true;
            });

        return services;
    }
}