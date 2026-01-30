using Budget.Application.Providers;
using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain;
using Budget.Domain.Commands;
using Budget.Domain.Entities;
using Budget.Domain.Enums;
using Budget.Domain.Messaging;
using Budget.Infrastructure.Database;
using Budget.Infrastructure.Database.Repositories;
using Budget.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Budget.Tests.Worker.Consumers;

public class TestableTransactionsFileConsumer(
    IServiceScopeFactory serviceScopeFactory,
    IMessageBusClient messageBusClient,
    ILogger<ProcessTransactionsFile> logger) : TransactionsFilesConsumer(serviceScopeFactory, messageBusClient, logger)
{
    public async Task CallExecute()
    {
        await base.ExecuteAsync(CancellationToken.None);
    }
}

[TestClass]
public class ProcessTransactionsFileConsumerTests(TestContext testContext) : BaseApiTests(testContext)
{
    private async Task<TestableTransactionsFileConsumer> CreateConsumerForAsync(TransactionsFileJob job,
        ILogger<ProcessTransactionsFile> logger, ITransactionsFileEtlUseCase? useCase = null)
    {
        var dbContext = await CreateContext(CreateUniqueUserName("consumer"));
        var transactionRepo = new TransactionRepository(dbContext);
        var contextMock = Substitute.For<ConsumeContext<ProcessTransactionsFile>>();
        contextMock.Message.Returns(new ProcessTransactionsFile { JobId = job.Id });
        var messageBusClient = Substitute.For<IMessageBusClient>();
        messageBusClient.SubscribeAsync<Guid>(MessageConstants.TransactionsFileJobCreated,
                "transactions-files-group")
            .Returns(new List<Guid> { job.Id }.ToAsyncEnumerable());
        
        var services = new ServiceCollection();
        services.AddScoped<BudgetDbContext>(_ => dbContext);
        services.AddScoped<IUserProvider>(_ => new ManualUserProvider("worker"));
        services.AddScoped<ITransactionsFileEtlUseCase>(_ =>
            useCase ?? new TransactionsFileEtlUseCase(transactionRepo,
                NullLogger<TransactionsFileEtlUseCase>.Instance));
        
        return new TestableTransactionsFileConsumer(
            services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>(),
            messageBusClient,
            logger);
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

        var consumer = await CreateConsumerForAsync(job, NullLogger<ProcessTransactionsFile>.Instance);

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

    [TestMethod]
    [DataRow(JobStatus.Completed)]
    [DataRow(JobStatus.Failed)]
    public async Task Consume_JobAlreadyCompletedOrFailed_DoesNotProcess(JobStatus status)
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<ProcessTransactionsFile>>();
        var username = CreateUniqueUserName("user1");
        var job = new TransactionsFileJob
        {
            Id = Guid.NewGuid(),
            Status = status,
            FileContent = await File.ReadAllBytesAsync("./Data/transactions-1.csv"),
            OriginalFileName = "transactions-1.csv",
            User = username
        };
        await using var db = await CreateContext(username);
        await db.TransactionsFileJobs.AddAsync(job, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);
        db.ChangeTracker.Clear();

        var consumer = await CreateConsumerForAsync(job, loggerMock);

        // Act
        await consumer.CallExecute();

        // Assert
        loggerMock.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString() == $"Job {job.Id} is already picked up by a previous process"),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());

        // Verify job status remains unchanged
        var unchangedJob = await db.TransactionsFileJobs.FindAsync([job.Id], CancellationToken.None);
        Assert.IsNotNull(unchangedJob);
        Assert.AreEqual(status, unchangedJob.Status);
    }

    [TestMethod]
    public async Task Consume_UseCaseFails_FailsAndSavesErrorInDb()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<ProcessTransactionsFile>>();
        var useCaseMock = Substitute.For<ITransactionsFileEtlUseCase>();
        useCaseMock.HandleAsync(Arg.Any<Stream>(), Arg.Any<string>())
            .Returns(Result.Failure("UseCase failed"));
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

        var consumer = await CreateConsumerForAsync(job, loggerMock, useCaseMock);

        // Act
        await consumer.CallExecute();

        // Assert
        var jobFailed = await db.TransactionsFileJobs.FirstOrDefaultAsync(j => j.Id == job.Id, CancellationToken.None);
        Assert.IsNotNull(jobFailed);
        Assert.AreEqual(JobStatus.Failed, jobFailed.Status);
        Assert.AreEqual("UseCase failed", jobFailed.ErrorMessage);
    }
}