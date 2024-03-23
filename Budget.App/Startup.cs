﻿using Budget.App.Apis.LoginLogout;
using Budget.App.Common;
using Budget.App.Config;
using Microsoft.Identity.Web;

namespace Budget.App;

public class Startup
{
    private readonly IConfiguration config;
    private readonly IWebHostEnvironment env;

    public Startup(IConfiguration config, IWebHostEnvironment env)
    {
        this.config = config;
        this.env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddMicrosoftIdentityConsentHandler();

        services
            .AddAuthentication(config)
            .AddServices()
            .AddProxyConfig(env);
    }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
    }
}
