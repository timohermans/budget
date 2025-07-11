using Budget.Core.DataAccess;

namespace Budget.Core.UseCases.Transactions.Overview;

public class OverviewRequest
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string? Iban { get; init; }
    public string? OrderBy { get; init; }
    public OrderDirection? Direction { get; init; }
}