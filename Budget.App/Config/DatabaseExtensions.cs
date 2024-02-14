using Budget.Core.DataAccess;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Budget.App.Config;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContextFactory<BudgetContext>(options =>
            options.UseNpgsql(config.GetConnectionString("BudgetContext"),
                b => b.MigrationsAssembly(typeof(BudgetContext).Assembly.FullName))
        );

        return services;
    }
}