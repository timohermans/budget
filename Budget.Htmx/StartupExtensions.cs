using Budget.Core.DataAccess;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Budget.Htmx.Config;
using Budget.Htmx.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Budget.Htmx;

public static class StartupExtensions
{
    internal static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContextFactory<BudgetContext>(options =>
            options.UseNpgsql(config.GetConnectionString("BudgetContext"),
                b => b.MigrationsAssembly(typeof(BudgetContext).Assembly.FullName))
        );
        return services;
    }

    public static IServiceCollection AddBudgetServices(this IServiceCollection services, IConfiguration config,
        IWebHostEnvironment env)
    {
        services
            .AddDatabase(config)
            .AddAuthentication(config)
            .AddServices()
            .AddEndpointsFrom(typeof(Program).Assembly);

        services.AddRazorComponents();
        
        return services;
    }

    public static IServiceCollection AddEndpointsFrom(this IServiceCollection services, Assembly assembly)
    {
        typeof(Program).Assembly
            .GetTypes()
            .Where(t => !t.IsInterface && t.IsAssignableTo(typeof(IEndpoint)))
            .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t))
            .ToList()
            .ForEach(services.TryAddEnumerable);
        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.Services.GetRequiredService<IEnumerable<IEndpoint>>()
            .ToList()
            .ForEach(e => e.Configure(app));
        return app;
    }

    public static WebApplication UseHtmxApplication(this WebApplication app, IWebHostEnvironment env)
    {
        
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
            app.UseForwardedHeaders();
        }

        app.UseStaticFiles();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.UseMiddleware<LogUsernameMiddleware>();

        app.MapEndpoints();
        return app;
    }
}