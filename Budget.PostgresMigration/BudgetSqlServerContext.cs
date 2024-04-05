using Budget.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Budget.PostgresMigration;

public class BudgetSqlServerContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder()
                          .AddJsonFile("appsettings.json")
                          .Build();

        optionsBuilder.UseSqlServer(config.GetConnectionString("SqlServer"));
    }
}