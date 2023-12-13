using Azure.Data.Tables;
using Budget.Core.Constants;
using Budget.Core.Models;
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
    .AddPolicy(AuthConstants.TwoFactorLoginPolicy, policy => policy.RequireClaim(AuthConstants.TwoFactorLoginPolicy, "2fa"));

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

// Add azure table storage
builder.Services.AddScoped(_ =>
{
    var service = new TableServiceClient(builder.Configuration.GetConnectionString("TransactionTable"));
    var client =  service.GetTableClient("Transactions");
    client.CreateIfNotExists();
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();