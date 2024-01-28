using Budget.Core.Constants;
using Budget.Core.DataAccess;
using Budget.Core.Infrastructure;
using Budget.Core.UseCases.Transactions.FileEtl;
using Budget.Pages.Pages;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// HTMX specific
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

// Authentication settings
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthConstants.TwoFactorLoginPolicy,
        policy => policy.RequireClaim(AuthConstants.TwoFactorLoginPolicy, "2fa"));

// Culture settings
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = ["nl"];
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Razor Pages settings
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddSingleton<LoginThrottler>();

// Add UseCases
var coreTypes = typeof(UseCase).Assembly.GetTypes();
var useCaseTypes = coreTypes
    .Where(t => t.Namespace != null && t.Namespace.Contains("Budget.Core.UseCases"))
    .ToList();
useCaseTypes
    .Where(t => t.Name.EndsWith("UseCase"))
    .ToList()
    .ForEach(t => builder.Services.AddScoped(t));

builder.Services.AddDbContext<BudgetContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BudgetContext"),
        b => b.MigrationsAssembly(typeof(IndexModel).Assembly.FullName))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program
{
}