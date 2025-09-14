using Budget.Application.Settings;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Domain.Entities;
using Budget.Domain.Messaging;
using Budget.Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Budget.Api.UnitTests.UseCases;

[TestClass]
public class TransactionsFileJobStartUseCaseTests
{
    private readonly ITransactionsFileJobRepository _repo = Substitute.For<ITransactionsFileJobRepository>();
    private readonly IMessageBusClient _endpoint = Substitute.For<IMessageBusClient>();

    private readonly ILogger<TransactionsFileJobStartUseCase> _logger =
        Substitute.For<ILogger<TransactionsFileJobStartUseCase>>();

    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();

    private readonly FileStorageSettings _fileSettings = new FileStorageSettings
    {
        MaxSizeMb = 10,
        BasePath = @"./budget-tests",
    };

    private TransactionsFileJobStartUseCase CreateSut()
    {
        if (_fileSettings.BasePath != null && !Path.Exists(_fileSettings.BasePath))
        {
            Directory.CreateDirectory(_fileSettings.BasePath);
        }

        return new(
            _repo,
            _logger,
            _endpoint,
            _fileSettings,
            _timeProvider);
    }

    [TestMethod]
    public async Task HandleAsync_ValidRequest_SavesJobAndPublishesEvent()
    {
        // Arrange
        TransactionsFileJob? capturedJob = null;
        _repo.When(x => x.AddAsync(Arg.Any<TransactionsFileJob>()))
            .Do(x => capturedJob = x.Arg<TransactionsFileJob>());
        var testTime = new DateTimeOffset(new DateTime(2025, 1, 1));
        _timeProvider.GetUtcNow().Returns(testTime);

        var sut = CreateSut();
        var validFile = new TransactionsFileJobStartUseCase.FileModel
        {
            Content = new byte[100],
            FileName = "valid.csv",
            ContentType = "text/csv",
            Size = 100
        };

        // Act
        var result = await sut.HandleAsync(new TransactionsFileJobStartCommand { File = validFile });

        // Assert
        Assert.IsTrue(result.IsSuccess);
        await _repo.Received(1).AddAsync(Arg.Is<TransactionsFileJob>(j =>
            j.CreatedAt == testTime.UtcDateTime));
        await _repo.Received(1).SaveChangesAsync();
        await _endpoint.Received(1).PublishAsync(MessageConstants.TransactionsFileJobCreated, Arg.Any<Guid?>());
        Assert.IsNotNull(capturedJob);
        Assert.AreNotEqual(Guid.Empty, capturedJob.Id);
        Assert.IsTrue(Math.Abs((testTime - capturedJob.CreatedAt).TotalMilliseconds) < 100, "CreatedAt is not within 100ms tolerance");
        Assert.IsTrue(capturedJob.FileContent.SequenceEqual(validFile.Content));
        Assert.AreEqual("valid.csv", capturedJob.OriginalFileName);
    }

    [TestMethod]
    public async Task HandleAsync_InvalidFileTooLarge_ReturnsFailure()
    {
        // Arrange
        _fileSettings.MaxSizeMb = 1;
        var sut = CreateSut();
        var invalidFile = new TransactionsFileJobStartUseCase.FileModel
        {
            Content = [],
            FileName = "test.csv",
            ContentType = "text/plain",
            Size = 999999999
        };

        // Act
        var result = await sut.HandleAsync(new TransactionsFileJobStartCommand { File = invalidFile });

        // Assert
        Assert.IsTrue(result.IsFailure);
        StringAssert.Contains(result.Error, "File size exceeds maximum");
    }
}