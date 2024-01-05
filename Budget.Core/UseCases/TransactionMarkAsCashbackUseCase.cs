using Azure.Data.Tables;
using Budget.Core.Infrastructure;
using Budget.Core.Models;
using Microsoft.Extensions.Logging;

namespace Budget.Core.UseCases;

public class TransactionMarkAsCashbackUseCase(TableClient table, ILogger<TransactionMarkAsCashbackUseCase> logger)
{
    public record Request(string RowKey, string PartitionKey, DateTime? Date);

    public record Response(Transaction transaction);

    public Result<Transaction> Handle(Request request)
    {
        var rowKey =
            request.RowKey
                .ToUpper(); // because of lowercase settings in program, the rowkey gets converted to lowercase
        var transaction = table.Query<Transaction>(t => t.PartitionKey == request.PartitionKey && t.RowKey == rowKey)
            .FirstOrDefault();

        if (transaction == null)
        {
            return new ErrorResult<Transaction>("Transaction not found");
        }

        Transaction transactionUpdated;

        if (request.Date.HasValue)
        {
            transactionUpdated = transaction with
            {
                PartitionKey = Transaction.CreatePartitionKey(request.Date.Value),
                CashbackForDate = DateTime.SpecifyKind(request.Date.Value, DateTimeKind.Utc)
            };
        }
        else
        {
            transactionUpdated = transaction with
            {
                PartitionKey = Transaction.CreatePartitionKey(transaction.DateTransaction),
                CashbackForDate = null
            };
        }

        table.DeleteEntity(request.PartitionKey, rowKey);

        try
        {
            table.AddEntity(transactionUpdated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while marking cashback for {Transaction} for date {request.Date}",
                transactionUpdated, request.Date);
            table.AddEntity(transaction);
            return new ErrorResult<Transaction>("Error while marking cashback");
        }

        logger.LogInformation(
            "Changed {Transaction} from {PartitionKey} to {NewPartitionKey} with cashback date {request.Date}",
            transaction.RowKey, request.PartitionKey, transactionUpdated.PartitionKey, request.Date);

        return new SuccessResult<Transaction>(transactionUpdated);
    }
}