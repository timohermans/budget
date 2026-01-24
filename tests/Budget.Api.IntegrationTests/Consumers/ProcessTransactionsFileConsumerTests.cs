using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain.Commands;
using Budget.Domain.Entities;
using Budget.Domain.Enums;
using Budget.Domain.Messaging;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Budget.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Budget.Api.IntegrationTests.Consumers;

public class TestableTransactionsFileConsumer(IMessageBusClient messageBusClient, BudgetDbContext db, ITransactionsFileEtlUseCase useCase, ILogger<ProcessTransactionsFile> logger) : TransactionsFilesConsumer(messageBusClient, db, useCase, logger)
{
    public async Task CallExecute()
    {
        await base.ExecuteAsync(CancellationToken.None);
    }
}

[TestClass]
public class ProcessTransactionsFileConsumerTests(TestContext testContext) : BaseApiTests(testContext)
{
    private readonly ILogger<ProcessTransactionsFile> loggerMock = NullLogger<ProcessTransactionsFile>.Instance;
    private readonly ConsumeContext<ProcessTransactionsFile> contextMock = Substitute.For<ConsumeContext<ProcessTransactionsFile>>();


    private async Task<TestableTransactionsFileConsumer> CreateConsumerForAsync(TransactionsFileJob job)
    {
        var dbContext = await CreateContext(CreateUniqueUserName("consumer"));
        var repo = new TransactionsFileJobRepository(dbContext);
        var transactionRepo = new TransactionRepository(dbContext);
        var useCase = new TransactionsFileEtlUseCase(transactionRepo, NullLogger<TransactionsFileEtlUseCase>.Instance);
        contextMock.Message.Returns(new ProcessTransactionsFile { JobId = job.Id });
        var messageBusClient = Substitute.For<IMessageBusClient>();
        messageBusClient.SubscribeAsync<Guid>(MessageConstants.TransactionsFileJobCreated,
            "transactions-files-group")
            .Returns(new List<Guid> { job.Id }.ToAsyncEnumerable());
        return new TestableTransactionsFileConsumer(messageBusClient, dbContext, useCase, loggerMock);
    }
    
    [TestMethod]
    public async Task Consume_GoodJobAndFile_CompletesJobSuccessfully()
    {
        // Arrange
        var username = CreateUniqueUserName("user1");
        var job = new TransactionsFileJob
        {
            Id = Guid.NewGuid(),
            Status = JobStatus.Pending,
            FileContent = await File.ReadAllBytesAsync("./Data/transactions-1.csv"),
            OriginalFileName = "transactions-1.csv",
            User = username
        };
        await using var db = await CreateContext(username);
        await db.TransactionsFileJobs.AddAsync(job, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);
        db.ChangeTracker.Clear();

        var consumer = await CreateConsumerForAsync(job);

        // Act
        await consumer.CallExecute();

        // Assert
        var updatedJob = await db.TransactionsFileJobs.FindAsync([job.Id], CancellationToken.None);
        Assert.IsNotNull(updatedJob);
        Assert.AreEqual(JobStatus.Completed, updatedJob.Status);
        Assert.IsNull(updatedJob.ErrorMessage);

        var transactions = db.Transactions.ToList();
        Assert.AreEqual(5, transactions.Count);
        
        Assert.IsTrue(transactions.All(t => t.User == username));
        Assert.AreEqual(4000.00m, transactions[0].Amount);
        Assert.AreEqual(2000.00m, transactions[1].Amount);
        Assert.AreEqual(-800.00m, transactions[2].Amount);
        Assert.AreEqual(-1000.00m, transactions[3].Amount);
        Assert.AreEqual(-90.75m, transactions[4].Amount);
    }
    
    // [TestMethod]
    // [DataRow(JobStatus.Completed)]
    // [DataRow(JobStatus.Failed)]
    // public async Task Consume_JobAlreadyCompletedOrFailed_DoesNotProcess(JobStatus status)
    // {
    //     // Arrange
    //     job.Status = status;
    //     var consumer = CreateConsumer();
    //
    //     // Act
    //     await consumer.CallExecute();
    //
    //     // Assert
    //     loggerMock.Received().LogInformation("Job is already picked up by a previous process");
    //     await repoMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    // }
}
