using Azure.Data.Tables;
using Budget.Core.Models;
using Microsoft.Extensions.Logging;

namespace Budget.Core.UseCases.Transactions.FileEtl;

public class TableClientDataAccess(TableClient db, ILogger<TableClientDataAccess> logger) : IDataAccess
{
    public async Task InsertMultipleAsync(IEnumerable<Transaction> transactions)
    {
        var transactionsPerPartitionKey = transactions
            .GroupBy(t => t.PartitionKey)
            .ToDictionary(kvp => kvp.Key, g => g.ToList());

        foreach (var partition in transactionsPerPartitionKey)
        {
            await Parallel.ForEachAsync(partition.Value.Chunk(100), async (transactionsChunk, cancelToken) =>
            {
                var addEntitiesBatch = transactionsChunk
                    .Select(transaction =>
                        new TableTransactionAction(TableTransactionActionType.UpsertMerge, transaction))
                    .ToList();

                var response = await db.SubmitTransactionAsync(addEntitiesBatch, cancelToken);

                logger.LogInformation("Saved {amount} transactions out of {count} in current batch",
                    response.Value.Count(r => r.Status == 204), transactionsChunk.Count());
            });
        }
    }
}