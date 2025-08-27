using Budget.ApiClient;
using Budget.Ui.Components;
using Budget.Ui.Server;
using Budget.Ui.Server.Middleware;
using Budget.Ui.Server.Options;
using Hertmans.Shared.Auth.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var services = builder.Services;
var environment = builder.Environment;

// Add services to the container.
services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(opts =>
    {
        opts.MaximumReceiveMessageSize = 100 * 1000; // 100kb for now. August was like 60kb
    });

services
 .AddSeriLogLogging(config)
 .AddOidcAuthentication(config)
 .AddProxyConfig(environment)
 .AddBudgetServices();

services.AddHttpContextAccessor();
services.AddApiClientRegistration<BudgetApiOptions>(config, "BudgetApi")
    .AddRefitClient<IBudgetClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.Run();
