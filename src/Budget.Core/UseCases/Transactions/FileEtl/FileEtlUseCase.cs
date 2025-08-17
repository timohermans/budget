using Budget.Core.DataAccess;
using Budget.Core.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Budget.ApiClient;
using Polly;
using Polly.Retry;
using Refit;

namespace Budget.Core.UseCases.Transactions.FileEtl;

public class FileEtlUseCase(IBudgetClient httpClient, ILogger<FileEtlUseCase> logger)
{
    public async Task<FileEtlResponse> HandleAsync(StreamPart stream)
    {
        var pipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().HandleResult(result =>
                {
                    if (result is TransactionsFileJobResponseModel jobResponse)
                    {
                        var isStillRunning = jobResponse.Status is "Processing" or "Pending";
                        return isStillRunning;
                    }

                    return false;
                }),
                OnRetry = args =>
                {
                    logger.LogInformation("Retrying to get finished transaction file job. Attempt {Attempt}",
                        args.AttemptNumber + 1);
                    return ValueTask.CompletedTask;
                },
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true, // Adds a random factor to the delay
                MaxRetryAttempts = 10,
                Delay = TimeSpan.FromSeconds(1),
            })
            .Build();

        logger.LogInformation("Sending transaction file to API");

        var result = await httpClient.PostTransactionsFileAsync(stream);
        logger.LogInformation("Successfully sent transaction file to API");

        var job = await pipeline.ExecuteAsync(async _ =>
        {
            var job = await httpClient.GetTransactionsFileJobAsync(result.JobId);
            return job;
        });

        return new FileEtlResponse(job.ErrorMessage);
    }
}