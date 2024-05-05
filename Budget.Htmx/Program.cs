using Budget.Htmx;
using Budget.Htmx.Config;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var env = builder.Environment;

builder.Host.AddAppLogging();

builder.Services.AddBudgetServices(config, env);

var app = builder.Build();

if (!env.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseForwardedHeaders();
}

app.UseDeveloperExceptionPage();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseMiddleware<LogUsernameMiddleware>();

app.MapEndpoints();

app.Run();