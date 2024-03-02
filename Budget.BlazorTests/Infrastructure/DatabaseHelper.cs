using Budget.Core.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Budget.BlazorTests.Infrastructure;

public class DatabaseHelper
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithDatabase("budget")
        .WithUsername("budget")
        .WithPassword("budget")
        .Build();

    private Respawner? respawner;

    public string ConnectionString => $"Server={container.Hostname};Port={container.GetMappedPublicPort(5432)};Database=budget;User Id=budget;Password=budget;";

    public BudgetContext CreateDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<BudgetContext>()
         .UseNpgsql(ConnectionString)
         .Options;

        return new BudgetContext(options);
    }

    public async Task ResetDataAsync()
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        if (respawner is not null)
        {
            await respawner.ResetAsync(conn);
        }
    }

    public async Task LaunchAsync()
    {
        await container.StartAsync();

        await CreateDbContextAsync().Database.MigrateAsync();

        using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            SchemasToInclude = ["public"],
            DbAdapter = DbAdapter.Postgres
        });
    }

    public async Task StopAsync()
    {
        await container.StopAsync();
        await container.DisposeAsync();
    }
}
