using Budget.Core.Extensions;
using Budget.Core.Models;
using Microsoft.EntityFrameworkCore;


namespace Budget.Core.DataAccess;

public class BudgetContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public BudgetContext(DbContextOptions options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        foreach(var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName().ToSnakeCase());

            foreach(var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }

            foreach(var key in entity.GetKeys())
            {
                key.SetName(key.GetName().ToSnakeCase());
            }

            foreach(var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
            }

            foreach(var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
            }
        }
    }
}