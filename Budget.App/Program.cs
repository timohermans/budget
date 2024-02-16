using Budget.App.Apis.LoginLogout;
using Budget.App.Common;
using Budget.App.Components;
using Budget.App.Config;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

    Log.Information("Starting application");

    var builder = WebApplication.CreateBuilder(args);
    var config = builder.Configuration;

    builder.Host.AddLogging(config);

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services
        .AddAuthentication(config)
        .AddDatabase(config)
        .AddServices()
        .AddProxyConfig(builder.Environment);

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
        app.UseForwardedHeaders();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.UseMiddleware<LogUsernameMiddleware>();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapGroup(LoginLogoutApi.GroupName)
        .MapLoginLogoutApis();

    app.Run();
    Log.Information("Stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}