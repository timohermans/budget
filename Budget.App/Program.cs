using Budget.App.Components;
using Budget.Core.DataAccess;
using Budget.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<LoginThrottler>();
builder.Services.AddSingleton(_ => TimeProvider.System);

builder.Services.AddDbContextFactory<BudgetContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BudgetContext"),
        b => b.MigrationsAssembly(typeof(BudgetContext).Assembly.FullName))
);
// TODO: Move migrations to Budget.Core and upate docs

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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
