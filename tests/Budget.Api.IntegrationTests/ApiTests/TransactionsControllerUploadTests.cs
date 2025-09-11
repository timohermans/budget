using System.Net.Http.Headers;
using System.Net.Http.Json;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain.Commands;
using Budget.Domain.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Budget.Api.IntegrationTests.ApiTests;

public class TransactionsControllerUploadTests(DatabaseAssemblyFixture fixture) : IClassFixture<DatabaseAssemblyFixture>
{
    [Fact]
    public async Task Upload_CorrectFile_SavesCorrectly()
    {
        Guid? publishedMessage = null;
        var publishEndpoint = Substitute.For<IMessageBusClient>();
        publishEndpoint.When(p => p.PublishAsync(MessageConstants.TransactionsFileJobCreated, Arg.Any<Guid>()))
            .Do(args => publishedMessage = args.Arg<Guid>());

        await using var app = await fixture.CreateApiApp(
            nameof(Upload_CorrectFile_SavesCorrectly),
            services =>
            {
                services.AddSingleton(publishEndpoint);
            },
            TestContext.Current.CancellationToken);
        var (client, db) = app;

        var fileStream = new MemoryStream(await File.ReadAllBytesAsync("Data/transactions-1.csv", TestContext.Current.CancellationToken));
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        using var formData = new MultipartFormDataContent
        {
            { fileContent, "file", "transactions.csv" }
        };

        var response = await client.PostAsync("/transactions/upload", formData, TestContext.Current.CancellationToken); // how to add a file 

        response.EnsureSuccessStatusCode();

        var jobResponse = await response.Content.ReadFromJsonAsync<TransactionsFileJobStartResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(jobResponse);
        var job = await db.TransactionsFileJobs.FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(job);
        Assert.Equal(job.Id, jobResponse?.JobId);
        await publishEndpoint.Received()
            .PublishAsync(MessageConstants.TransactionsFileJobCreated, Arg.Any<Guid>());
        Assert.Null(job.ErrorMessage);
        Assert.Equal("transactions.csv", job.OriginalFileName);
        Assert.True(fileStream.ToArray().SequenceEqual(job.FileContent));
        Assert.NotNull(publishedMessage);
        Assert.Equivalent(job.Id, publishedMessage);
    }
}