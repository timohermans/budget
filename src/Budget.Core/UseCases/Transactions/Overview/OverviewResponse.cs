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
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Iban { get; set; } = string.Empty;
    public string? IbanOtherParty { get; set; }
    public string? NameOtherParty { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }
    public bool IsFixed { get; set; }
    public bool IsFromOwnAccount { get; set; }
    public bool IsFromOtherParty { get; set; }
    public DateTime? CashbackForDate { get; set; }
    public DateTime OriginalDate { get; set; }

    public OverviewTransaction()
    {
    }

    public OverviewTransaction(Transaction transaction, IEnumerable<string> ibansOwned)
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
        IsFromOtherParty = transaction.IsFromOtherParty(ibansOwned);
        IsFromOwnAccount = transaction.IsFromOwnAccount(ibansOwned);
        CashbackForDate = transaction.CashbackForDate.HasValue ? transaction.CashbackForDate.Value.ToDateTime(default) : null;
        OriginalDate = transaction.OriginalDate.ToDateTime(default);
    }
}