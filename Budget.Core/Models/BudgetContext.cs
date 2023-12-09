using Microsoft.EntityFrameworkCore;

namespace Budget.Core.Models;

public class BudgetContext(DbContextOptions<BudgetContext> contextOptions) : DbContext(contextOptions)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
}