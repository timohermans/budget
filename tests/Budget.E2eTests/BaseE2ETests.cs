using Budget.Infrastructure.Database;
using Budget.Application.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Npgsql;

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

public class BaseE2ETests(TestContext testContext)
{
    protected static Uri AppUrl { get; private set; } = null!;

    private IPlaywright _playwright = null!;
    protected IBrowser _browser = null!;

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task BeforeAll(TestContext testContext)
    {
        Environment.SetEnvironmentVariable("PWDEBUG", "0");

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        AppUrl = new Uri("localhost:5001");
    }

    [TestInitialize]
    public async Task BeforeEach()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            ExecutablePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
            Headless = false,
        });

    }

    [TestCleanup]
    public async Task AfterEach()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    /// <summary>
    /// Creates a username that's unique for a single unit test
    /// </summary>
    /// <param name="userName">the username you'd like to add (e.g. "Toby")</param>
    /// <returns>Username prefixed with the test name</returns>
    protected string CreateUniqueUserName(string userName)
    {
        var userNameBuilder = new List<string> { testContext.TestName ?? "test" };
        if (testContext.TestData is { Length: > 0 })
        {
            userNameBuilder.Add(string.Join("_", testContext.TestData.Select(o => o?.ToString())));
        }

        userNameBuilder.Add(userName);
        return string.Join("_", userNameBuilder);
    }

    /// <summary>
    /// Creates a DB context with a UserProvider for the provided username.
    /// </summary>
    /// <param name="userName">The user that will be injected and used for queries.</param>
    /// <returns>the context</returns>
    protected async Task<BudgetDbContext> CreateContext(string userName)
    {
        var connectionString = "Host=localhost;Post=5122;Username=postgres;Password=postgres;Database=budgetdb";
        var db = new BudgetDbContext(
            new DbContextOptionsBuilder<BudgetDbContext>()
                .UseNpgsql(connectionString)
                .Options, new TestUserProvider(userName));

        await db.Database.MigrateAsync();
        return db;
    }
    
    protected async Task AuthenticateUserAsync(IPage page, string userName)
    {
        await page.FillAsync("[name='username']", userName);
        await page.ClickAsync("button[type='submit']");
    }

    private class TestUserProvider(string username) : IUserProvider
    {
        public string? GetCurrentUser() => username;
    }
}