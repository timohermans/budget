using Budget.Core.Infrastructure;
using Budget.Core.Models;
using Budget.Core.UseCases;
using Budget.IntegrationTests.Config;
using Budget.IntegrationTests.Helpers;
using FakeItEasy;
using Xunit.Abstractions;

namespace Budget.IntegrationTests.Tests.Core.UseCases;

[Collection("integration")]
public class TransactionMarkAsCashbackUseCaseTests(TestFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public async Task Mark_transaction_as_cashback()
    {
        var timeProvider = A.Fake<TimeProvider>();
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(new DateTime(2023, 12, 1));
        var db = await fixture.CreateTableClientAsync();

        await new TransactionFileUploadUseCase(db, new XunitLogger<TransactionFileUploadUseCase>(output))
            .HandleAsync(File.OpenRead("Data/transactions-3.csv"));

        var transactionToMark = db.Query<Transaction>(q => q.Amount == 100).First();

        transactionToMark.PartitionKey.Should().Be("2024-1");

        var actual = new TransactionMarkAsCashbackUseCase(db, new XunitLogger<TransactionMarkAsCashbackUseCase>(output))
            .Handle(
                new TransactionMarkAsCashbackUseCase.Request(transactionToMark.RowKey,
                    transactionToMark.PartitionKey,
                    new DateTime(2024, 1, 2)));

        actual.Should().BeAssignableTo<SuccessResult<Transaction>>();
        actual.Data.PartitionKey.Should().Be("2024-1");
    }
}