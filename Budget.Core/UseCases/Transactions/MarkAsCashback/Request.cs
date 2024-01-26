namespace Budget.Core.UseCases.Transactions.MarkAsCashback;

public record Request(string RowKey, string PartitionKey, DateTime? Date);
