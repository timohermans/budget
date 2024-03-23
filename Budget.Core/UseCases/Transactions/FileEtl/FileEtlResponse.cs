namespace Budget.Core.UseCases.Transactions.FileEtl;

public record FileEtlResponse(int AmountInserted, DateTime DateMin, DateTime DateMax);
