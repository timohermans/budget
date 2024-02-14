using Budget.App.Apis.LoginLogout;
using Budget.App.Components;
using Budget.Core.Constants;
using Budget.Core.DataAccess;
using Budget.Core.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("Auth:Authority");
        options.ClientId = builder.Configuration.GetValue<string>("Auth:ClientId");
        options.ClientSecret = builder.Configuration.GetValue<string>("Auth:ClientSecret");
        options.ResponseType = "id_token token";
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username"
        };
    });


builder.Services.AddValidatorsFromAssemblyContaining<LoginModel.Validator>();
builder.Services.AddSingleton<LoginThrottler>();
builder.Services.AddSingleton(_ => TimeProvider.System);

builder.Services.AddDbContextFactory<BudgetContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BudgetContext"),
        b => b.MigrationsAssembly(typeof(BudgetContext).Assembly.FullName))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/account")
    .MapLoginLogoutApis(); 

app.Run();
