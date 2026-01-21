using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain.Commands;
using Budget.Domain.Entities;
using Budget.Domain.Enums;
using Budget.Domain.Messaging;
using Budget.Domain.Repositories;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Budget.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Budget.Api.IntegrationTests.Consumers;

public class TestableTransactionsFileConsumer(IMessageBusClient messageBusClient, ITransactionsFileJobRepository repo, ITransactionsFileEtlUseCase useCase, ILogger<ProcessTransactionsFile> logger) : TransactionsFilesConsumer(messageBusClient, repo, useCase, logger)
{
    public async Task CallExecute()
    {
        await base.ExecuteAsync(CancellationToken.None);
    }
}

[TestClass]
public class ProcessTransactionsFileConsumerTests
{
    private readonly ILogger<ProcessTransactionsFile> loggerMock;
    private readonly ConsumeContext<ProcessTransactionsFile> contextMock;
    private readonly TransactionsFileJob job = new TransactionsFileJob
    {
        Id = Guid.NewGuid(),
        Status = JobStatus.Pending,
        FileContent = File.ReadAllBytes("./Data/transactions-1.csv"),
        OriginalFileName = "transactions-1.csv",
        User = "testuser"
    };

    public ProcessTransactionsFileConsumerTests()
    {
        loggerMock = NullLogger<ProcessTransactionsFile>.Instance;
        contextMock = Substitute.For<ConsumeContext<ProcessTransactionsFile>>();
    }

    private TestableTransactionsFileConsumer CreateConsumer(BudgetDbContext dbContext)
    {
        var repo = new TransactionsFileJobRepository(dbContext);
        var transactionRepo = new TransactionRepository(dbContext);
        var useCase = new TransactionsFileEtlUseCase(transactionRepo, NullLogger<TransactionsFileEtlUseCase>.Instance);
        contextMock.Message.Returns(new ProcessTransactionsFile { JobId = job.Id });
        var messageBusClient = Substitute.For<IMessageBusClient>();
        messageBusClient.SubscribeAsync<Guid>(MessageConstants.TransactionsFileJobCreated,
            "transactions-files-group")
            .Returns(new List<Guid> { job.Id }.ToAsyncEnumerable());
        return new TestableTransactionsFileConsumer(messageBusClient, repo, useCase, loggerMock);
    }

    [TestMethod]
    public async Task Consume_GoodJobAndFile_CompletesJobSuccessfully()
    {
        // Arrange
        await using var db = await BaseApiTests.CreateContext(nameof(Consume_GoodJobAndFile_CompletesJobSuccessfully));
        await db.Database.BeginTransactionAsync(CancellationToken.None);
        await db.TransactionsFileJobs.AddAsync(job, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        var consumer = CreateConsumer(db);

        // Act
        await consumer.CallExecute();

        // Assert
        var updatedJob = await db.TransactionsFileJobs.FindAsync([job.Id], CancellationToken.None);
        Assert.IsNotNull(updatedJob);
        Assert.AreEqual(JobStatus.Completed, updatedJob.Status);
        Assert.IsNull(updatedJob.ErrorMessage);

        var transactions = db.Transactions.ToList();
        Assert.AreEqual(5, transactions.Count); // Assuming there are 5 transactions in the CSV file

        // Additional assertions to verify the transactions
        Assert.AreEqual(4000.00m, transactions[0].Amount);
        Assert.AreEqual(2000.00m, transactions[1].Amount);
        Assert.AreEqual(-800.00m, transactions[2].Amount);
        Assert.AreEqual(-1000.00m, transactions[3].Amount);
        Assert.AreEqual(-90.75m, transactions[4].Amount);
    }
}
