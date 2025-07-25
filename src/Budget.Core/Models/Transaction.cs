﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Budget.Core.Models;

[DebuggerDisplay("{DateTransaction} {Iban} <- {NameOtherParty} {Amount} {Description}")]
public class Transaction
{
    private readonly string[] _otherPartiesAlwaysNotFixed = ["paypal"];
    private DateOnly _dateTransaction;

    public int Id { get; set; }
    public int FollowNumber { get; set; }
    [StringLength(34)]
    public required string Iban { get; set; }
    [StringLength(5)]
    public required string Currency { get; set; }
    [Precision(12, 2)]
    public decimal Amount { get; set; }

    [BackingField(nameof(_dateTransaction))]
    public DateOnly DateTransaction
    {
        get => CashbackForDate ?? _dateTransaction;
        set => _dateTransaction = value;
    }

    public DateOnly OriginalDate => _dateTransaction;

    [Precision(12, 2)]
    public decimal BalanceAfterTransaction { get; set; }
    [StringLength(255)]
    public string? NameOtherParty { get; set; }
    [StringLength(34)]
    public string? IbanOtherParty { get; set; }
    [StringLength(255)]
    public string? AuthorizationCode { get; set; }
    [StringLength(255)]
    public string? Description { get; set; }
    public DateOnly? CashbackForDate { get; set; }

    public bool IsIncome => Amount > 0;
    public bool IsFixed => !string.IsNullOrWhiteSpace(AuthorizationCode) && _otherPartiesAlwaysNotFixed.All(p => !string.Equals(p, NameOtherParty, StringComparison.InvariantCultureIgnoreCase));
    public bool IsFromOtherParty(IEnumerable<string> ibansOwned) => !ibansOwned.Contains(IbanOtherParty);
    public bool IsFromOwnAccount(IEnumerable<string> ibansOwned) => ibansOwned.Contains(IbanOtherParty);
    public bool IsFromThisMonth(DateTimeOffset date) => DateTransaction.Year == date.Year && DateTransaction.Month == date.Month;
    

}