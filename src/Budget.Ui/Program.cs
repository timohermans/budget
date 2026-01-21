using System.Security.Claims;
using Budget.ApiClient;
using Budget.Ui.Components;
using Budget.Ui.Server;
using Budget.Ui.Server.Middleware;
using Budget.Ui.Server.Options;
using Hertmans.Shared.Auth.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
    .AddOidcAuthentication(config, environment)
    .AddProxyConfig(environment)
    .AddBudgetServices();

services.AddHttpContextAccessor();
services.AddApiClientRegistration<BudgetApiOptions>(config, "BudgetApi",
        environment.IsDevelopment() ? CookieAuthenticationDefaults.AuthenticationScheme : "OpenIdConnect")
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
else
{
    app.MapPost("/api/fake-login", async ([FromForm] string username, HttpContext context) =>
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(ClaimTypes.Name, username),
            new("name", username),
            new("fake_jwt", "this-is-a-mock-jwt-token")
        };

        var identity =
            new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        var fakeJwt = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"fake-jwt-for-{username}"));

        var authProperties = new AuthenticationProperties();
        authProperties.StoreTokens([
            new AuthenticationToken { Name = "access_token", Value = fakeJwt }
        ]);

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        return Results.Redirect("/");
    });
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