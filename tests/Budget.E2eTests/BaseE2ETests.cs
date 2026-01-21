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
        var connectionString = "Host=localhost;Post=5122;Username=postgres;Password=postgres;Database=budgetdb";

        await using var dbContext = new BudgetDbContext(
            new DbContextOptionsBuilder<BudgetDbContext>()
                .UseNpgsql(connectionString)
                .Options);

        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.Transactions
            .Where(t => t.User.StartsWith("test_"))
            .ExecuteDeleteAsync();

        await dbContext.TransactionsFileJobs
            .Where(t => t.User.StartsWith("test_"))
            .ExecuteDeleteAsync();
    }
}