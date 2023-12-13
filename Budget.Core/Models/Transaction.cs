using Azure;
using Azure.Data.Tables;

namespace Budget.Core.Models;

public class Transaction : ITableEntity
{
    public int Id { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.Now;
    public ETag ETag { get; set; } = default!;
    public required int FollowNumber { get; set; }
    public required string Iban { get; set; }
    public required string Currency { get; set; }
    public double Amount { get; set; }
    public required DateTime DateTransaction { get; set; }
    public double BalanceAfterTransaction { get; set; }
    public string? NameOtherParty { get; set; }
    public string? IbanOtherParty { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? Description { get; set; }
    public DateTime? CashbackForDate { get; set; }
    public bool IsIncome => Amount > 0;
    public bool IsFixed => !string.IsNullOrWhiteSpace(AuthorizationCode);
    public bool IsFromOtherParty(IEnumerable<string> ibansOwned) => !ibansOwned.Contains(IbanOtherParty);
    public bool IsFromOwnAccount(IEnumerable<string> ibansOwned) => ibansOwned.Contains(IbanOtherParty);
}