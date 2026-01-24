using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain.Commands;
using Budget.Domain.Enums;
using Budget.Domain.Messaging;
using Budget.Domain.Repositories;
using Budget.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Budget.Worker.Consumers;

public class TransactionsFilesConsumer(
    IMessageBusClient messageBusClient,
    BudgetDbContext db,
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
            await ProcessJob(jobId, stoppingToken);
        }

        logger.LogInformation("Exiting transactions file jobs...");
    }

    private async Task ProcessJob(Guid jobId, CancellationToken stoppingToken)
    {
        try
        {
            if (jobId == null) return;

            logger.LogInformation("Going to process transactions file job {JobId}", jobId);

            var job = await db.TransactionsFileJobs
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(j => j.Id == jobId, stoppingToken);

            if (job == null || job.Status is JobStatus.Completed or JobStatus.Failed)
            {
                logger.LogInformation("Job {JobId} is already picked up by a previous process", jobId);
                return;
            }

            logger.LogInformation("Start processing transactions file job {JobId}", jobId);

            job.Status = JobStatus.Processing;
            await db.SaveChangesAsync(stoppingToken);

            if (job.FileContent.Length == 0)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "File content is empty";
                logger.LogWarning("Job {JobId} failed with message: {result}", jobId, job.ErrorMessage);
                await db.SaveChangesAsync(stoppingToken);
                return;
            }

            await using var fileStream = new MemoryStream(job.FileContent);

            var result = await useCase.HandleAsync(fileStream, job.User);

            if (result.IsFailure)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = result.Error;
                logger.LogWarning("Job {JobId} failed with message: {result}", jobId, result.Error);
            }
            else
            {
                job.Status = JobStatus.Completed;
            }

            await db.SaveChangesAsync(stoppingToken);

            logger.LogInformation("Finished processing transactions file job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while trying to process transactions file job {JobId}", jobId);
        }
    }
}