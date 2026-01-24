using Budget.Infrastructure.Database;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Budget.Api.IntegrationTests.Utils.Providers;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Budget.Api.IntegrationTests;

public class Sut(BudgetDbContext Db, HttpClient Client, IServiceScope scope) : IAsyncDisposable
{
    public void Deconstruct(out HttpClient client, out BudgetDbContext db)
    {
        client = Client;
        db = Db;
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        Client.Dispose();
        scope.Dispose();
    }
}

[TestClass]
public class BaseApiTests(TestContext testContext)
{
    private static readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder().Build();

    private static bool UseTestContainers =>
        Environment.GetEnvironmentVariable("USE_TESTCONTAINERS")?.Equals("true", StringComparison.OrdinalIgnoreCase) ==
        true
        || Environment.GetEnvironmentVariable("CI") != null;

    private static string? ConnectionString => UseTestContainers
        ? PostgreSqlContainer.GetConnectionString()
        : "Host=localhost;Port=5122;Database=budgetdb;Username=postgres;Password=password";

    [AssemblyInitialize]
    public static async Task InitializeAsync(TestContext testContext)
    {
        if (UseTestContainers)
        {
            await PostgreSqlContainer.StartAsync(testContext.CancellationTokenSource.Token);
        }
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (UseTestContainers)
        {
            await PostgreSqlContainer.DisposeAsync();
        }
    }

    [TestInitialize]
    public async Task BeforeEach()
    {
        await using var db = await CreateContext("base");
        if (testContext.TestName == null) return;
        await db.Transactions.IgnoreQueryFilters().Where(t => t.User.StartsWith(testContext.TestName))
            .ExecuteDeleteAsync(testContext.CancellationTokenSource.Token);
        await db.TransactionsFileJobs.IgnoreQueryFilters().Where(t => t.User.StartsWith(testContext.TestName))
            .ExecuteDeleteAsync(testContext.CancellationTokenSource.Token);
    }

    protected Task<Sut> CreateSut(CancellationToken token = default) =>
        CreateSut("testuser", null, token);

    protected async Task<Sut> CreateSut(string userName,
        Action<IServiceCollection>? configureTestServicesFn = null,
        CancellationToken token = default)
    {
        var testName = testContext.TestName ?? "unknown-test";
        var clientFactory =
            await CustomWebApplicationFactory<Program>.CreateApiClientAsync(ConnectionString, testName, token,
                userName);
        var scope = clientFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
        var client = clientFactory
            .WithWebHostBuilder(builder =>
            {
                if (configureTestServicesFn != null)
                {
                    builder.ConfigureTestServices(configureTestServicesFn);
                }
            })
            .CreateClient();

        return new Sut(db, client, scope);
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
        var dbName = "budgetdb";
        var apiConnectionString = new NpgsqlConnectionStringBuilder(ConnectionString)
        {
            Database = dbName
        }.ToString();
        var db = new BudgetDbContext(
            new DbContextOptionsBuilder<BudgetDbContext>()
                .UseNpgsql(apiConnectionString)
                .Options, new TestUserProvider(userName));

        await db.Database.MigrateAsync();
        return db;
    }
}