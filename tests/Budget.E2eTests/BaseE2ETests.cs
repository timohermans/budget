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

        AppUrl = new Uri("localhost:5001");
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