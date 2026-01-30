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

    /// <summary>
    /// This method will log in the user after navigating to the first url provided.
    /// </summary>
    /// <remarks>ONLY USE THIS when logging in for the first time, as it assumes you need to log in initially</remarks>
    /// <param name="page"></param>
    /// <param name="url"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public static async Task GoToWithAuthenticationAsync(this IPage page, string url, string username)
    {
        await page.GotoWithIdleWaitAsync(url);
        await AuthenticateUserAsync(page, username);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="page"></param>
    /// <param name="userName"></param>
    private static async Task AuthenticateUserAsync(IPage page, string userName)
    {
        await page.FillAsync("[name='username']", userName);
        await page.ClickAsync("button[type='submit']");
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

        // AppUrl = new Uri("localhost:5001");
        AppUrl = new Uri("https://localhost:7110");
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

        await using var db = await CreateContext("base");
        if (testContext.TestName == null)
        {
            throw new InvalidOperationException("Test name should never be null?");
        }

        await db.Transactions.IgnoreQueryFilters().Where(t => t.User.StartsWith(testContext.TestName))
            .ExecuteDeleteAsync(testContext.CancellationTokenSource.Token);
        await db.TransactionsFileJobs.IgnoreQueryFilters().Where(t => t.User.StartsWith(testContext.TestName))
            .ExecuteDeleteAsync(testContext.CancellationTokenSource.Token);
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
        var connectionString = "Host=localhost;Port=5122;Username=postgres;Password=password;Database=budgetdb";
        var db = new BudgetDbContext(
            new DbContextOptionsBuilder<BudgetDbContext>()
                .UseNpgsql(connectionString)
                .Options, new TestUserProvider(userName));

        await db.Database.MigrateAsync();
        return db;
    }

    private class TestUserProvider(string initialUsername) : IUserProvider
    {
        public string? GetCurrentUser() => initialUsername;

        public void OverrideUser(string username)
        {
        }
    }
}