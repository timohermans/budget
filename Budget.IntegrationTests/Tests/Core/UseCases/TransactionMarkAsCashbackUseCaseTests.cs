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

        var result = new TransactionMarkAsCashbackUseCase(db, new XunitLogger<TransactionMarkAsCashbackUseCase>(output))
            .Handle(
                new TransactionMarkAsCashbackUseCase.Request(transactionToMark.RowKey,
                    transactionToMark.PartitionKey,
                    new DateTime(2024, 1, 2)));

        result.Should().BeAssignableTo<SuccessResult<Transaction>>();
        result.Data.RowKey.Should().Be(transactionToMark.RowKey);
        var actual = db.Query<Transaction>(q => q.RowKey == transactionToMark.RowKey).First();
        actual.PartitionKey.Should().Be("2024-1");
        actual.CashbackForDate.Should().Be(new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task Unmark_cashback_transaction()
    {
        var db = await fixture.CreateTableClientAsync();
        var date = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var cashbackDate = new DateTime(2023, 12, 10, 0, 0, 0, DateTimeKind.Utc);
        var transactionToUnmark = new Transaction
        {
            Currency = "EUR",
            Iban = "NL22RABO0101044666",
            DateTransaction = date,
            FollowNumber = 1121,
            PartitionKey = Transaction.CreatePartitionKey(cashbackDate),
            RowKey = "NL22RABO0101044666-1121",
            Amount = 151,
            Description = "Cambrium Tweak BV",
            IbanOtherParty = "NL33INGB0000000000",
            AuthorizationCode = "CODE66",
            BalanceAfterTransaction = 1000,
            CashbackForDate = cashbackDate,
            NameOtherParty = "Cambrium BV",
        };
        await db.AddEntityAsync(transactionToUnmark);

        var actual = new TransactionMarkAsCashbackUseCase(db, new XunitLogger<TransactionMarkAsCashbackUseCase>(output))
            .Handle(
                new TransactionMarkAsCashbackUseCase.Request(transactionToUnmark.RowKey,
                    transactionToUnmark.PartitionKey,
                    null));

        actual.Should().BeAssignableTo<SuccessResult<Transaction>>();

        var transaction = db.Query<Transaction>(q => q.RowKey == transactionToUnmark.RowKey).First();

        transaction.Should().NotBeNull();
        transaction.PartitionKey.Should().Be(Transaction.CreatePartitionKey(date),
            "the partition key decides in which month the transaction is shown");
        transaction.CashbackForDate.Should().BeNull();
    }
}