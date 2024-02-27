namespace Budget.Core.UseCases.Transactions.FileEtl;

public record Response(int AmountInserted, DateOnly DateMin, DateOnly DateMax);
