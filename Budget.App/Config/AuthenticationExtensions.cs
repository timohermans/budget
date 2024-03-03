using Microsoft.Identity.Web;

namespace Budget.App.Config;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
    {
        //services.AddAuthentication(options =>
        //    {
        //        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        //    })
        //    .AddCookie()
        //    .AddOpenIdConnect(options =>
        //    {
        //        options.Authority = config.GetValue<string>("Auth:Authority");
        //        options.ClientId = config.GetValue<string>("Auth:ClientId");
        //        options.ClientSecret = config.GetValue<string>("Auth:ClientSecret");
        //        options.ResponseType = "id_token token";
        //        options.SaveTokens = true;
        //        options.Scope.Add("openid");
        //        options.Scope.Add("profile");
        //        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //        options.TokenValidationParameters = new TokenValidationParameters
        //        {
        //            NameClaimType = "preferred_username",
        //            RoleClaimType = "roles"
        //        };
        //    });

        services.AddMicrosoftIdentityWebAppAuthentication(config, configSectionName: "AzureAd");
        services.AddAuthorization();

        return services;
    }
}