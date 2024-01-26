namespace Budget.Core.UseCases.Transactions.FileEtl;

public record Response(int AmountInserted, DateTime DateMin, DateTime DateMax);
