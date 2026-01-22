using Budget.Application.Providers;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Budget.Infrastructure.Database;

public class BudgetDbContext(DbContextOptions<BudgetDbContext> options, IUserProvider userProvider) : DbContext(options)
{
    private readonly IUserProvider _userProvider = userProvider;

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionsFileJob> TransactionsFileJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<Transaction>().HasQueryFilter(t => t.User == _userProvider.GetCurrentUser());
        modelBuilder.Entity<TransactionsFileJob>().HasQueryFilter(j => j.User == _userProvider.GetCurrentUser());
    }
}