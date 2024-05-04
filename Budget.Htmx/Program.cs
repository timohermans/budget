using Budget.Htmx;
using Budget.Htmx.Config;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var env = builder.Environment;

builder.Host.AddAppLogging();

builder.Services.AddBudgetServices(config, env);

var app = builder.Build();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();