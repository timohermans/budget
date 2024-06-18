using Budget.Core.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Budget.BlazorTests.DatabaseMigration;

public class MsSqlToPostgresActions
{
    [Test]
    [Ignore("only an action, no real test")]
    public void Migrate_data_from_mssql_to_postgres()
    {
        var postgresOptions = new DbContextOptionsBuilder<BudgetPgContext>()
            .UseNpgsql("Host=localhost;User Id=postgres;Password=p@ssw0rd;Database=budget")
            .Options;

        var postgresDb = new BudgetPgContext(postgresOptions);

        postgresDb.Database.EnsureCreated();


        var options = new DbContextOptionsBuilder<BudgetContext>()
            .UseSqlServer(
                "Server=localhost;User Id=sa;Password=to7R@U9R3Ms9q&;Database=budget;TrustServerCertificate=True")
            .Options;

        var sqlServerDb = new BudgetContext(options);
        sqlServerDb.Database.EnsureCreated();

        var transactions = sqlServerDb.Transactions.ToList();

        postgresDb.Transactions.AddRange(transactions);
        postgresDb.SaveChanges();
    }
}