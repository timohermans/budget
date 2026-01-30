using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Budget.Ui.Core.Constants;

namespace Budget.Ui.Core.Models;

[DebuggerDisplay("{DateTransaction} {Iban} <- {NameOtherParty} {Amount} {Description}")]
public class Transaction
{
    private readonly string[] _otherPartiesAlwaysNotFixed = ["paypal"];

    private readonly string[] _fixedCodes =
    [
        RabobankTransactionCodes.Bankgiro,
        RabobankTransactionCodes.Diversen,
        RabobankTransactionCodes.EuroIncasso,
    ];

    private DateOnly _dateTransaction;

    public int Id { get; set; }
    public int FollowNumber { get; set; }
    [StringLength(34)] public required string Iban { get; set; }
    [StringLength(5)] public required string Currency { get; set; }
    public decimal Amount { get; set; }

    public DateOnly DateTransaction
    {
        get => CashbackForDate ?? _dateTransaction;
        set => _dateTransaction = value;
    }

    public DateOnly OriginalDate => _dateTransaction;

    public decimal BalanceAfterTransaction { get; set; }
    public string? NameOtherParty { get; set; }
    public string? IbanOtherParty { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? Description { get; set; }
    public DateOnly? CashbackForDate { get; set; }

    /// <summary>
    /// Gets or sets the code of the transaction. Using it "sb" which marks the salary
    /// </summary>
    public string? Code { get; set; }

    public bool IsIncome => Amount > 0;

    public bool IsFixed => _fixedCodes.Any(c => c == Code) && _otherPartiesAlwaysNotFixed.All(p =>
        !(NameOtherParty?.Contains(p, StringComparison.InvariantCultureIgnoreCase) ?? false));

    public bool IsFromOtherParty(IEnumerable<string> ibansOwned) => !ibansOwned.Contains(IbanOtherParty);
    public bool IsFromOwnAccount(IEnumerable<string> ibansOwned) => ibansOwned.Contains(IbanOtherParty);

    public bool IsFromThisMonth(DateTimeOffset date) =>
        DateTransaction.Year == date.Year && DateTransaction.Month == date.Month;
}