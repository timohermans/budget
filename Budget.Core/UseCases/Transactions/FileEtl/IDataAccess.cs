using Budget.Core.Models;

namespace Budget.Core.UseCases.Transactions.FileEtl;

public interface IDataAccess
{
    Task InsertMultipleAsync(IEnumerable<Transaction> transactions);
}