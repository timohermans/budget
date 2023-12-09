using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Budget.Core.Models;

public class Transaction
{
    public int Id { get; set; }
    public int FollowNumber { get; set; }
    [StringLength(34)]
    public required string Iban { get; set; }
    [StringLength(5)]
    public required string Currency { get; set; }
    [Precision(12, 2)]
    public decimal Amount { get; set; }
    public DateOnly DateTransaction { get; set; }
    [Precision(12, 2)]
    public decimal AmountAfterTransaction { get; set; }
    [StringLength(255)]
    public string? NameOtherParty { get; set; }
    [StringLength(34)]
    public string? IbanOtherParty { get; set; }
    [StringLength(255)]
    public string? AuthorizationCode { get; set; }
    [StringLength(255)]
    public string? Description { get; set; }
}