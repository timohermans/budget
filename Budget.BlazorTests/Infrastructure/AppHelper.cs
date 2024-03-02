using Budget.Core.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Budget.BlazorTests.Infrastructure;

internal class AppHelper
{
    private IHost? host;
    private DatabaseHelper databaseHelper;

    public AppHelper(DatabaseHelper databaseHelper)
    {
        this.databaseHelper = databaseHelper;
    }

    public async Task<Uri> LaunchAsync(string url)
    {

        var builder = Program.BuildWebHost();
        builder.UseEnvironment("Test");
        builder.ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseUrls(url);
        });
        builder.ConfigureServices(services =>
        {
            var descriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BudgetContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<BudgetContext>(options =>
                   options.UseNpgsql(databaseHelper.ConnectionString));
        });
        host = builder.Build();

        await host.StartAsync();

        return new(host.Services.GetRequiredService<IServer>().Features
            .GetRequiredFeature<IServerAddressesFeature>()
            .Addresses.Single());
    }

    internal async Task StopAsync()
    {
        if (host is not null)
        {
            await host.StopAsync(TimeSpan.FromSeconds(2));
            host.Dispose();
        }
    }
}
