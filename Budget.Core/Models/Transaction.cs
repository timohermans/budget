namespace Budget.Core.Models;

public class Transaction
{
    public int Id { get; set; }
    public int FollowNumber { get; set; }
    public string Iban { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DateTransaction { get; set; }
    public decimal AmountAfterTransaction { get; set; }
    public string NameOtherParty { get; set; }
    public string IbanOtherParty { get; set; }
    public string AuthorizationCode { get; set; }
    public string Description { get; set; }
}