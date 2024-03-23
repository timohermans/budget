using Budget.App.Apis.LoginLogout;
using Budget.App.Common;
using Budget.App.Config;
using Microsoft.Identity.Web;

namespace Budget.App;

public static class StartupExtensions
{
    public static IServiceCollection AddBudgetServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddMicrosoftIdentityConsentHandler();

        services
            .AddAuthentication(config)
            .AddServices()
            .AddProxyConfig(env);
        return services;
    }
    
    public static IApplicationBuilder UseBudgetApp(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
            app.UseForwardedHeaders();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.UseMiddleware<LogUsernameMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorComponents<Components.App>()
                .AddInteractiveServerRenderMode();

            endpoints.MapGroup(LoginLogoutApi.GroupName)
                .MapLoginLogoutApis();
        });
        return app;
    }
}