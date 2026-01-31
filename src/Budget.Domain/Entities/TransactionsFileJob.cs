using Budget.Domain.Enums;

namespace Budget.Domain.Entities;

public class TransactionsFileJob
{
    public const string StreamName = "TRANSACTION_FILES";
    public Guid Id { get; set; }
    public required string OriginalFileName { get; set; }
    public required byte[] FileContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public string? ErrorMessage { get; set; }
    public required string User {get; set; }
}