using System.Net;
using Budget.Api;
using Budget.Api.Controllers;
using Budget.App;
using Budget.Core.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Budget.BlazorTests.Infrastructure;

internal class AppHelper
{
    private WebApplication? _app;
    private WebApplication? _apiApp;
    private readonly DatabaseHelper _databaseHelper;

    public AppHelper(DatabaseHelper databaseHelper)
    {
        _databaseHelper = databaseHelper;
    }

    public Uri Launch(string url, string urlOfApi)
    {
        LaunchApi();
        return LaunchApp();
    }

    private void LaunchApi()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions()
            {
                ContentRootPath = GetProjectDirectory("Budget.Api").FullName,
                ApplicationName = "Budget.Api"
            });
        AddAdditionalAppSettings<TransactionController>(builder, "Budget.Api", builder.Environment.EnvironmentName);

        builder.WebHost.UseUrls("http://localhost:5078");
        builder.Services.AddAllApiServices(builder.Configuration);
        ReplaceDatabaseWithTest(builder);

        _apiApp = builder.Build();
        _apiApp.UseBudgetApi(builder.Environment.IsDevelopment(), builder.Configuration);

        _apiApp.Start();
    }

    private void ReplaceDatabaseWithTest(WebApplicationBuilder builder)
    {
        var descriptor =
            builder.Services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BudgetContext>));
        if (descriptor != null)
        {
            builder.Services.Remove(descriptor);
        }

        builder.Services.AddDbContext<BudgetContext>(options =>
            options.UseNpgsql(_databaseHelper.ConnectionString));
    }

    private Uri LaunchApp()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions()
            {
                ContentRootPath = GetProjectDirectory("Budget.App").FullName,
                ApplicationName = "Budget.App"
            });
        AddAdditionalAppSettings<Startup>(builder, "Budget.App", builder.Environment.EnvironmentName);

        var startup = new Startup(builder.Configuration, builder.Environment);
        builder.WebHost.UseUrls("http://localhost:5223");
        startup.ConfigureServices(builder.Services);
        _app = builder.Build();
        startup.Configure(_app, _app.Environment);

        _app.Start();

        return new(_app.Services.GetRequiredService<IServer>().Features
            .GetRequiredFeature<IServerAddressesFeature>()
            .Addresses.Single());
    }

    private static void AddAdditionalAppSettings<T>(WebApplicationBuilder builder, string projectName, string environment) where T : class
    {
        var projectDir = GetProjectDirectory(projectName).FullName;
        var appsettingsDir = Path.Combine(projectDir, projectName);
        builder.Configuration.AddConfiguration(
            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(appsettingsDir, "appsettings.json"), false)
                .AddJsonFile(Path.Combine(appsettingsDir, $"appsettings.{environment}.json"), true)
                .AddUserSecrets<T>()
                .Build());
    }

    private static DirectoryInfo GetProjectDirectory(string projectName)
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory() ??
                                    throw new NullReferenceException("current directory not found"));
        var budgetApiDirName = projectName;
        while (dir!.GetDirectories().ToList().TrueForAll(d => d.Name != budgetApiDirName))
        {
            dir = dir.Parent;
        }

        return dir;
    }

    internal async Task StopAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync(TimeSpan.FromSeconds(2));
            await _app.DisposeAsync();
        }

        if (_apiApp is not null)
        {
            await _apiApp.StopAsync(TimeSpan.FromSeconds(2));
            await _apiApp.DisposeAsync();
        }
    }
}