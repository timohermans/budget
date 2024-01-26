namespace Budget.Core.UseCases.Transactions.Overview;

public class Request
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string? Iban { get; init; }
}