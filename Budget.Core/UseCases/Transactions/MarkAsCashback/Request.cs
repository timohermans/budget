namespace Budget.Core.UseCases.Transactions.MarkAsCashback;

public record Request(int Id, DateOnly? Date);
