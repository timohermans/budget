using Budget.Application.Providers;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Budget.Infrastructure.Database;

public class BudgetDbContext(DbContextOptions<BudgetDbContext> options, IUserProvider userProvider) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionsFileJob> TransactionsFileJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<Transaction>().HasQueryFilter(t => t.User == userProvider.GetCurrentUser());
        modelBuilder.Entity<TransactionsFileJob>().HasQueryFilter(j => j.User == userProvider.GetCurrentUser());
    }
}