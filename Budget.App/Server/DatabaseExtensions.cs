using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Budget.App.Server;

public static class DatabaseExtensions
{
    public static IServiceCollection AddPostgresDatabase<T>(this IServiceCollection services, IConfiguration config) where T : DbContext
    {
        services.AddDbContextFactory<T>(options =>
            options.UseNpgsql(config.GetConnectionString("BudgetContext"),
                b => b.MigrationsAssembly(typeof(T).Assembly.FullName))
        );

        return services;
    }
}