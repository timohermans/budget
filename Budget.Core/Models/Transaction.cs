using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Azure;
using Azure.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Budget.Core.Models;

[Index(nameof(FollowNumber), nameof(Iban), IsUnique = true)]
public class Transaction : ITableEntity
{
    private DateTime _dateTransaction;

    public int Id { get; set; }
    [NotMapped]
    public required string PartitionKey { get; set; }
    [NotMapped]
    public required string RowKey { get; set; }
    [NotMapped]
    public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.Now;
    [NotMapped]
    public ETag ETag { get; set; } = default!;
    public required int FollowNumber { get; set; }
    [StringLength(34)]
    public required string Iban { get; set; }
    [StringLength(5)]
    public required string Currency { get; set; }
    [Precision(12, 2)]
    public double Amount { get; set; }
    public required DateTime DateTransaction
    {
        get => _dateTransaction;
        set
        {
            _dateTransaction = value;
        }
    }
    [Precision(12, 2)]
    public double BalanceAfterTransaction { get; set; }
    [StringLength(255)]
    public string? NameOtherParty { get; set; }
    [StringLength(34)]
    public string? IbanOtherParty { get; set; }
    [StringLength(255)]
    public string? AuthorizationCode { get; set; }
    [StringLength(255)]
    public string? Description { get; set; }

    [NotMapped]
    public DateTime? CashbackForDate { get; set; }

    public bool IsIncome => Amount > 0;
    public bool IsFixed => !string.IsNullOrWhiteSpace(AuthorizationCode);

    public bool IsFromOtherParty(IEnumerable<string> ibansOwned) => !ibansOwned.Contains(IbanOtherParty);
    public bool IsFromOwnAccount(IEnumerable<string> ibansOwned) => ibansOwned.Contains(IbanOtherParty);



}