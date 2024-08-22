using Budget.Core.DataAccess;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Budget.App.Server;

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(_ => TimeProvider.System);

        return services;
    }
}