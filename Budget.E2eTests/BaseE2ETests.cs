using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Budget.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Budget.E2eTests;

public static class PageExtensions
{
    public static async Task<IResponse?> GotoWithIdleWaitAsync(this IPage page, string url)
    {
        return await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }
}

public class BaseE2ETests : PageTest
{
    private static DistributedApplication _app = null!;
    protected static Uri AppUrl { get; private set; } = null!;

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task BeforeAll(TestContext testContext)
    {
        Environment.SetEnvironmentVariable("HEADED", "0");
        Environment.SetEnvironmentVariable("PWDEBUG", "0");
        
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Budget_Aspire>(testContext.CancellationToken);

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var app = await builder.BuildAsync(testContext.CancellationToken);
        await app.StartAsync(testContext.CancellationToken);
        // var httpClient = app.CreateHttpClient("webfrontend");
        // var response = await httpClient.GetAsync("/");

        var resourceNotificationService =
            app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService
            .WaitForResourceAsync("budget-app", KnownResourceStates.Running, testContext.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(30), testContext.CancellationToken);

        _app = app;
        AppUrl = app.GetEndpoint("budget-app");
    }

    [TestInitialize]
    public async Task BeforeEach()
    {
        var connectionString = await _app.GetConnectionStringAsync("budget-db");

        await using var dbContext = new BudgetDbContext(
            new DbContextOptionsBuilder<BudgetDbContext>()
                .UseNpgsql($"{connectionString};Database=budgetdb")
                .Options);

        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Transactions.ExecuteDeleteAsync();
        await dbContext.TransactionsFileJobs.ExecuteDeleteAsync();
    }

    [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task AfterAll(TestContext testContext)
    {
        await _app.StopAsync(testContext.CancellationToken);
        await _app.DisposeAsync();
    }
}