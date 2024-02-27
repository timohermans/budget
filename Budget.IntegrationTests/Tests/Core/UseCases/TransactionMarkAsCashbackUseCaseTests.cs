using Budget.Core.Infrastructure;
using Budget.Core.Models;
using Budget.Core.UseCases.Transactions.MarkAsCashback;
using FileEtl = Budget.Core.UseCases.Transactions.FileEtl;
using Budget.IntegrationTests.Config;
using Budget.IntegrationTests.Helpers;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
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

        await new FileEtl.UseCase(db, new XunitLogger<FileEtl.UseCase>(output))
            .HandleAsync(File.OpenRead("Data/transactions-3.csv"));

        var transactionToMark = await db.Transactions.FirstOrDefaultAsync(q => q.Amount == 100);

        transactionToMark.Should().NotBeNull();
        transactionToMark!.DateTransaction.Should().Be(new DateOnly(2024, 1, 4));

        var result = await new UseCase(db, new XunitLogger<UseCase>(output))
            .Handle(new Request(transactionToMark.Id, new DateOnly(2024, 1, 2)));

        result.Should().BeAssignableTo<SuccessResult<Transaction>>();
        result.Data.Id.Should().Be(transactionToMark.Id);
        var actual = await db.Transactions.FirstOrDefaultAsync(q => q.Id == transactionToMark.Id);
        actual?.DateTransaction.Should().Be(new DateOnly(2024, 1, 2));
        actual?.CashbackForDate.Should().Be(new DateOnly(2024, 1, 2));
    }

    [Fact]
    public async Task Unmark_cashback_transaction()
    {
        var db = await fixture.CreateTableClientAsync();
        var date = new DateOnly(2024, 1, 5);
        var cashbackDate = new DateOnly(2023, 12, 10);
        var transactionToUnmark = new Transaction
        {
            Currency = "EUR",
            Iban = "NL22RABO0101044666",
            DateTransaction = date,
            FollowNumber = 1121,
            Amount = 151,
            Description = "Cambrium Tweak BV",
            IbanOtherParty = "NL33INGB0000000000",
            AuthorizationCode = "CODE66",
            BalanceAfterTransaction = 1000,
            CashbackForDate = cashbackDate,
            NameOtherParty = "Cambrium BV",
        };
        await db.Transactions.AddAsync(transactionToUnmark);
        await db.SaveChangesAsync();

        var actual = await new UseCase(db, new XunitLogger<UseCase>(output))
            .Handle(new Request(transactionToUnmark.Id, null));

        actual.Should().BeAssignableTo<SuccessResult<Transaction>>();

        var transaction = await db.Transactions.FirstAsync(q => q.Id == transactionToUnmark.Id);

        transaction.Should().NotBeNull();
        transaction.DateTransaction.Should()
            .Be(date, "the partition key decides in which month the transaction is shown");
        transaction.CashbackForDate.Should().BeNull();
    }
}