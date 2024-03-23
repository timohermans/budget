using Budget.Core.Models;

namespace Budget.Core.UseCases.Transactions.Overview;

public class OverviewResponse
{
    public required string IbanSelected { get; init; }
    public required IEnumerable<string> IbansToSelect { get; init; }
    public required DateTime Date { get; init; }
    public required DateTime DatePreviousMonth { get; init; }
    public required decimal ExpensesFixedLastMonth { get; init; }
    public required decimal IncomeLastMonth { get; init; }
    public required List<int> WeeksInMonth { get; init; }
    public required decimal ExpensesVariable { get; init; }
    public required Dictionary<int, decimal> ExpensesPerWeek { get; init; }
    public required Dictionary<string, decimal> BalancePerAccount { get; init; }
    public required decimal IncomeFromOwnAccounts { get; init; }
    public required List<OverviewTransaction> Transactions { get; init; }
    public required decimal BudgetAvailable { get; init; }
    public required decimal BudgetPerWeek { get; init; }
}

public class OverviewTransaction
{
    public int Id { get; private set; }
    public DateTime Date { get; private set; }
    public string Iban { get; private set; }
    public string? IbanOtherParty { get; private set; }
    public string? NameOtherParty { get; private set; }
    public string? Description { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsIncome { get; private set; }
    public bool IsFixed { get; private set; }
    public bool IsFromOwnAccount { get; private set; }
    public bool IsFromOtherParty { get; private set; }
    public bool IsCashback { get; private set; }
    public DateTime? CashbackForDate { get; private set; }
    public DateTime OriginalDate { get; private set; }

    public OverviewTransaction(Transaction transaction)
    {
        Id = transaction.Id;
        Date = transaction.DateTransaction.ToDateTime(default);
        Iban = transaction.Iban;
        IbanOtherParty = transaction.IbanOtherParty;
        NameOtherParty = transaction.NameOtherParty;
        Description = transaction.Description;
        Amount = transaction.Amount;
        IsIncome = transaction.IsIncome;
        IsFixed = transaction.IsFixed;
        CashbackForDate = transaction.CashbackForDate.HasValue ? transaction.CashbackForDate.Value.ToDateTime(default) : null;
        OriginalDate = transaction.OriginalDate.ToDateTime(default);
    }
}