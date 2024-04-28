using Budget.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Budget.Core.DataAccess;

public class BudgetContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public BudgetContext(DbContextOptions options) : base(options)
    {
    }
}