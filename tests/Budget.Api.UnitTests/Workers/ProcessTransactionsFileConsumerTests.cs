using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain;
using Budget.Domain.Commands;
using Budget.Domain.Entities;
using Budget.Domain.Enums;
using Budget.Domain.Messaging;
using Budget.Domain.Repositories;
using Budget.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Budget.Api.UnitTests.Workers
{
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
        private readonly ITransactionsFileJobRepository repoMock;
        private readonly ITransactionsFileEtlUseCase useCaseMock;
        private readonly IMessageBusClient messageBusClient;
        private readonly ILogger<ProcessTransactionsFile> loggerMock;
        private readonly ConsumeContext<ProcessTransactionsFile> contextMock;
        private readonly TransactionsFileJob job = new()
        {
            Id = Guid.NewGuid(),
            Status = JobStatus.Pending,
            FileContent = new byte[1000],
            OriginalFileName = "file.txt"
        };


        public ProcessTransactionsFileConsumerTests()
        {
            repoMock = Substitute.For<ITransactionsFileJobRepository>();
            useCaseMock = Substitute.For<ITransactionsFileEtlUseCase>();
            loggerMock = Substitute.For<ILogger<ProcessTransactionsFile>>();
            contextMock = Substitute.For<ConsumeContext<ProcessTransactionsFile>>();
            messageBusClient = Substitute.For<IMessageBusClient>();
        }

        private TestableTransactionsFileConsumer CreateConsumer()
        {
            repoMock.GetByIdAsync(Arg.Any<Guid>()).Returns(job);
            contextMock.Message.Returns(new ProcessTransactionsFile { JobId = job.Id });
            messageBusClient.SubscribeAsync<Guid>(MessageConstants.TransactionsFileJobCreated,
                "transactions-files-group")
                .Returns(new List<Guid> { job.Id }.ToAsyncEnumerable());
            return new TestableTransactionsFileConsumer(messageBusClient, repoMock, useCaseMock, loggerMock);
        }

        [TestMethod]
        public async Task Consume_GoodJobAndFile_CompletesJobSuccessfully()
        {
            // Arrange
            useCaseMock.HandleAsync(Arg.Any<Stream>()).Returns(Result.Success());
            var consumer = CreateConsumer();

            // Act
            await consumer.CallExecute();

            // Assert
            Assert.AreEqual(JobStatus.Completed, job.Status);
            Assert.IsNull(job.ErrorMessage);
        }

        [TestMethod]
        [DataRow(JobStatus.Completed)]
        [DataRow(JobStatus.Failed)]
        public async Task Consume_JobAlreadyCompletedOrFailed_DoesNotProcess(JobStatus status)
        {
            // Arrange
            job.Status = status;
            var consumer = CreateConsumer();

            // Act
            await consumer.CallExecute();

            // Assert
            loggerMock.Received().LogInformation("Job is already picked up by a previous process");
            await repoMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task Consume_UseCaseFails_FailsAndSavesErrorInDb()
        {
            // Arrange
            var consumer = CreateConsumer();
            useCaseMock.HandleAsync(Arg.Any<Stream>()).Returns(Result.Failure("UseCase failed"));

            // Act
            await consumer.CallExecute();

            // Assert
            Assert.AreEqual(JobStatus.Failed, job.Status);
            Assert.AreEqual("UseCase failed with message: UseCase failed", job.ErrorMessage);
        }
    }
}