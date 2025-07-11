using Budget.App.Components;
using Budget.App.Server;
using Budget.App.Server.Middleware;
using Budget.Core.DataAccess;
using Budget.Core.UseCases.Transactions.Overview;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
var host = builder.Host;
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
 .AddPostgresDatabase<BudgetContext>(config)
 .AddProxyConfig(environment)
 .AddBudgetServices();

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
app.MapGroup("account").MapLoginAndLogout();


app.Run();
