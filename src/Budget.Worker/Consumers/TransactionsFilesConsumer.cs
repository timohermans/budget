using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain.Commands;
using Budget.Domain.Enums;
using Budget.Domain.Messaging;
using Budget.Domain.Repositories;

namespace Budget.Worker.Consumers;

public class TransactionsFilesConsumer(
    IMessageBusClient messageBusClient,
    ITransactionsFileJobRepository repo,
    ITransactionsFileEtlUseCase useCase,
    ILogger<ProcessTransactionsFile> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subscribing to transactions file jobs...");
        await foreach (var jobId in messageBusClient.SubscribeAsync<Guid>(
                               MessageConstants.TransactionsFileJobCreated, "transactions-files-group")
                           .WithCancellation(stoppingToken))
        {
            if (jobId == null) continue;

            logger.LogInformation("Going to process transactions file job {JobId}", jobId);
            var job = await repo.GetByIdAsync(jobId);

            if (job == null || job.Status is JobStatus.Completed or JobStatus.Failed)
            {
                logger.LogInformation("Job is already picked up by a previous process");
                return;
            }

            job.Status = JobStatus.Processing;
            await repo.SaveChangesAsync(stoppingToken);

            if (job.FileContent.Length == 0)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "File content is empty";
                await repo.SaveChangesAsync(stoppingToken);
                return;
            }

            await using var fileStream = new MemoryStream(job.FileContent);

            var result = await useCase.HandleAsync(fileStream);

            if (result.IsFailure)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = $"UseCase failed with message: {result.Error}";
            }
            else
            {
                job.Status = JobStatus.Completed;
            }

            await repo.SaveChangesAsync(stoppingToken);
        }

        logger.LogInformation("Exiting transactions file jobs...");
    }
}