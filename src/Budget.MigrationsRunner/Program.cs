using Budget.Application.Providers;
using Budget.Infrastructure;
using Budget.Infrastructure.Database;
using Budget.MigrationsRunner.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

try
{
    var builder = WebApplication.CreateBuilder();
    var config = builder.Configuration;
    var services = builder.Services;
    
    services.AddInfrastructure(config);
    services.AddScoped<IUserProvider, DummyUserProvider>();

    var app = builder.Build();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
    await context.Database.MigrateAsync();
    Console.WriteLine("Migrations applied successfully");

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Migration failed: {ex.Message}");
    return 1; // Kubernetes will detect non-zero exit as failure
}