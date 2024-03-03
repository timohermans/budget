using Microsoft.Identity.Web;

namespace Budget.App.Config;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddMicrosoftIdentityWebAppAuthentication(config, configSectionName: "AzureAd");
        services.AddAuthorization();

        return services;
    }
}