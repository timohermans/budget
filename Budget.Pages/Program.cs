using Azure.Data.Tables;
using Azure.Identity;
using Budget.Core.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

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

// Add UseCases
typeof(Budget.Core.UseCases.TransactionFileUploadUseCase).Assembly.GetTypes()
    .Where(t => t.Namespace == "Budget.Core.UseCases")
    .Where(t => t.Name.EndsWith("UseCase"))
    .ToList()
    .ForEach(t => builder.Services.AddScoped(t));

// Add azure table storage
builder.Services.AddScoped(_ =>
{
    var service = new TableServiceClient(builder.Configuration.GetConnectionString("TransactionTable"));
    var client = service.GetTableClient("Transactions");
    client.CreateIfNotExists();
    return client;
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection()
        .SetApplicationName("Budget")
        .PersistKeysToAzureBlobStorage(builder.Configuration.GetConnectionString("Storage"), "keys", "keys.xml");
}

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