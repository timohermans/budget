using Budget.Core.Models;

namespace Budget.Core.UseCases.Transactions.Overview;

public class Response
{
    public required string IbanSelected { get; init; }
    public required IEnumerable<string> IbansToSelect { get; init; }
    public required DateOnly Date { get; init; }
    public required DateOnly DatePreviousMonth { get; init; }
    public required decimal ExpensesFixedLastMonth { get; init; }
    public required decimal IncomeLastMonth { get; init; }
    public required List<int> WeeksInMonth { get; init; }
    public required decimal ExpensesVariable { get; init; }
    public required Dictionary<int, decimal> ExpensesPerWeek { get; init; }
    public required Dictionary<string, decimal> BalancePerAccount { get; init; }
    public required decimal IncomeFromOwnAccounts { get; init; }
    public required Dictionary<int, List<Transaction>> TransactionsPerWeek { get; init; }
}