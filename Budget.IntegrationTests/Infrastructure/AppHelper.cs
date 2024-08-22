using Budget.App.Components.Pages;
using Budget.App.Server;
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
    private readonly DatabaseHelper _databaseHelper;

    public AppHelper(DatabaseHelper databaseHelper)
    {
        _databaseHelper = databaseHelper;
    }

    public Uri Launch(string url, string urlOfApi)
        => LaunchApp();

    private Uri LaunchApp()
    {
        const string ProjectName = "Budget.App";
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions()
            {
                ContentRootPath = GetProjectDirectory(ProjectName).FullName,
                ApplicationName = ProjectName
            });
        AddAdditionalAppSettings<Home>(builder, ProjectName,
            builder.Environment.EnvironmentName);
        var config = builder.Configuration;
        var environment = builder.Environment;

        builder.WebHost.UseUrls("http://localhost:5223");
        builder.Services.AddSeriLogLogging(config)
                .AddOidcAuthentication(config)
                .AddPostgresDatabase<BudgetContext>(config)
                .AddProxyConfig(environment)
                .AddBudgetServices();

        ReplaceDatabaseWithTest(builder);

        _app = builder.Build();

        //_app.UseHtmxApplication(builder.Environment);
        // TODO: fix this

        _app.Start();

        return new(_app.Services.GetRequiredService<IServer>().Features
            .GetRequiredFeature<IServerAddressesFeature>()
            .Addresses.Single());
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

    private static void AddAdditionalAppSettings<T>(WebApplicationBuilder builder, string projectName,
        string environment) where T : class
    {
        var projectDir = GetProjectDirectory(projectName).FullName;
        var appsettingsDir = Path.Combine(projectDir, projectName);
        builder.Configuration.AddConfiguration(
            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(appsettingsDir, "appsettings.json"), false)
                .AddJsonFile(Path.Combine(appsettingsDir, $"appsettings.{environment}.json"), false)
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
    }
}