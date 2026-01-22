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
public class BaseApiTests
{
    private static readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder().Build();
    private static string? ConnectionString => PostgreSqlContainer.GetConnectionString();

    [AssemblyInitialize]
    public static async Task InitializeAsync(TestContext testContext)
    {
        await PostgreSqlContainer.StartAsync(testContext.CancellationTokenSource.Token);
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        await PostgreSqlContainer.DisposeAsync();
    }

    protected Task<Sut> CreateSut(string testName, CancellationToken token = default) =>
        CreateSut(testName, "testuser", null, token);

    protected async Task<Sut> CreateSut(string testName, string userName, Action<IServiceCollection>? configureTestServicesFn = null,
        CancellationToken token = default)
    {
        var clientFactory =
            await CustomWebApplicationFactory<Program>.CreateApiClientAsync(ConnectionString, testName, token, userName);
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


    public static async Task<BudgetDbContext> CreateContext(string testName, string userName = "testuser")
    {
        var dbName = $"{testName.Substring(0, 27)}_{Guid.NewGuid().ToString().Replace("-", "_")}".ToLower();
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

    private static Task SeedDataAsync(BudgetDbContext context)
    {
        // Fill when necessary
        // context.AddRange(
        //     new Blog { Name = "Blog1", Url = "http://blog1.com" },
        //     new Blog { Name = "Blog2", Url = "http://blog2.com" });
        return Task.CompletedTask;
    }


}