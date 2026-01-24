using System.Net.Http.Headers;
using System.Net.Http.Json;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Budget.Tests.Api.ApiTests;

[TestClass]
public class TransactionsControllerUploadTests(TestContext testContext) : BaseApiTests(testContext)
{
    [TestMethod]
    public async Task Upload_CorrectFile_SavesCorrectly()
    {
        Guid? publishedMessage = null;
        var publishEndpoint = Substitute.For<IMessageBusClient>();
        publishEndpoint.When(p => p.PublishAsync(MessageConstants.TransactionsFileJobCreated, Arg.Any<Guid>()))
            .Do(args => publishedMessage = args.Arg<Guid>());
        var username = CreateUniqueUserName("Test user");

        await using var app = await CreateSut(
            username,
            services => services.AddSingleton(publishEndpoint),
            CancellationToken.None);
        var (client, db) = app;

        var fileStream =
            new MemoryStream(await File.ReadAllBytesAsync("Data/transactions-1.csv", CancellationToken.None));
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        using var formData = new MultipartFormDataContent
        {
            { fileContent, "file", "transactions.csv" }
        };

        var response = await client.PostAsync("/transactions/upload", formData, CancellationToken.None);

        response.EnsureSuccessStatusCode();

        var jobResponse =
            await response.Content.ReadFromJsonAsync<TransactionsFileJobStartResponse>(
                cancellationToken: CancellationToken.None);
        Assert.IsNotNull(jobResponse);
        var job = await db.TransactionsFileJobs.FirstOrDefaultAsync(cancellationToken: CancellationToken.None);
        Assert.IsNotNull(job);
        Assert.AreEqual(job.Id, jobResponse?.JobId);
        await publishEndpoint.Received()
            .PublishAsync(MessageConstants.TransactionsFileJobCreated, Arg.Any<Guid>());
        Assert.AreEqual(job.User, username);
        Assert.IsNull(job.ErrorMessage);
        Assert.AreEqual("transactions.csv", job.OriginalFileName);
        Assert.IsTrue(fileStream.ToArray().SequenceEqual(job.FileContent));
        Assert.IsNotNull(publishedMessage);
        Assert.AreEqual(job.Id, publishedMessage);
    }
}