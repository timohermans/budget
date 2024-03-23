using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Budget.App.Config;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddMicrosoftIdentityWebAppAuthentication(config, configSectionName: "AzureAd")
            .EnableTokenAcquisitionToCallDownstreamApi(["api://c6876c39-e090-411d-8284-8154c6e050b3/access_as_user"])
                .AddDownstreamApi("Budget Api", config.GetSection("Api"))
                .AddInMemoryTokenCaches();

        services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        })
        .AddMicrosoftIdentityUI();

        services.AddAuthorization();

        return services;
    }
}