using Budget.Application.Settings;
using Budget.Domain;
using Budget.Domain.Entities;
using Budget.Domain.Messaging;
using Budget.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Budget.Application.UseCases.TransactionsFileJobStart;

public interface ITransactionsFileJobStartUseCase
{
    Task<Result<TransactionsFileJobStartResponse>> HandleAsync(TransactionsFileJobStartCommand command);
}

public class TransactionsFileJobStartUseCase(
    ITransactionsFileJobRepository repo,
    ILogger<TransactionsFileJobStartUseCase> logger,
    IMessageBusClient messageBusClient,
    FileStorageSettings fileSettings,
    TimeProvider timeProvider) : ITransactionsFileJobStartUseCase
{
    public class FileModel
    {
        public required byte[] Content { get; init; }
        public required string FileName { get; init; }
        public required string ContentType { get; init; }
        public long Size { get; init; }
    }

    public async Task<Result<TransactionsFileJobStartResponse>> HandleAsync(TransactionsFileJobStartCommand command)
    {
        var fileValidator = new TransactionsFileValidator(fileSettings, logger);

        var validateResult = fileValidator.IsValid(command.File);

        if (validateResult.IsFailure)
        {
            return Result<TransactionsFileJobStartResponse>.Failure(validateResult.Error);
        }

        var job = new TransactionsFileJob
        {
            Id = Guid.NewGuid(),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            FileContent = command.File.Content,
            OriginalFileName = command.File.FileName,
        };
        await repo.AddAsync(job);
        await repo.SaveChangesAsync();

        await messageBusClient.PublishAsync(MessageConstants.TransactionsFileJobCreated, data: job.Id);

        return Result<TransactionsFileJobStartResponse>.Success(new TransactionsFileJobStartResponse
        {
            JobId = job.Id
        });
    }
}

public class TransactionsFileJobStartCommand
{
    public required TransactionsFileJobStartUseCase.FileModel File { get; init; }
}

public class TransactionsFileJobStartResponse
{
    public Guid JobId { get; set; }
}
