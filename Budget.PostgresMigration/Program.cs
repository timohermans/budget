using Budget.Core.DataAccess;
using Budget.PostgresMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

var postgresOptions = new DbContextOptionsBuilder<BudgetContext>()
                            .UseNpgsql(config.GetConnectionString("Postgres"))
                            .Options;
var dbPostgres = new BudgetContext(postgresOptions);

Console.WriteLine($"Fetching psql transactions...");

var transactions = await dbPostgres.Transactions.ToListAsync();

Console.WriteLine($"Fetched {transactions.Count} transactions");

var dbSqlServer = new BudgetSqlServerContext();

transactions.ForEach(t =>
{
    t.Id = 0;
});

await dbSqlServer.Transactions.AddRangeAsync(transactions);

await dbSqlServer.SaveChangesAsync();

Console.WriteLine("Inserted all transactions");

Console.WriteLine("Done migrating!");
