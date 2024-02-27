using Budget.Core.Models;
using Budget.Core.UseCases.Transactions.Overview;
using Budget.IntegrationTests.Config;

namespace Budget.IntegrationTests.Tests.UseCases.Transactions;

[Collection("integration")]
public class OverviewTests(TestFixture fixture)
{
    [Fact]
    public async Task Keeps_check_of_how_much_each_own_account_saved()
    {
        var paymentIban = "NL22OWN01010100";
        var savingsIban = "NL33OWN01010100";
        var savingsIban2 = "NL44OWN01010100";
        var storeIban = "NL55OTHER01010100";
        var date = new DateOnly(2023, 12, 1);
        var baseTransaction = new Transaction
        {
            Currency = "EUR",
            Iban = paymentIban,
            IbanOtherParty = savingsIban,
            DateTransaction = date,
            FollowNumber = 1,
            Id = 1,
            Amount = -100,
            NameOtherParty = "Savings 1"
        };
        List<Transaction> transactions =
        [
            CreateFrom(baseTransaction, 2, -100, paymentIban, savingsIban),
            CreateFrom(baseTransaction, 3, 100, savingsIban, paymentIban),
            CreateFrom(baseTransaction, 4, -200, paymentIban, savingsIban),
            CreateFrom(baseTransaction, 5, 200, savingsIban, paymentIban),
            CreateFrom(baseTransaction, 6, 50, paymentIban, savingsIban),
            CreateFrom(baseTransaction, 7, -50, savingsIban, paymentIban),
            // savings2
            CreateFrom(baseTransaction, 8, -1000, paymentIban, savingsIban2),
            CreateFrom(baseTransaction, 9, 1000, savingsIban2, paymentIban),
            CreateFrom(baseTransaction, 10, -2000, paymentIban, savingsIban2),
            CreateFrom(baseTransaction, 11, 2000, savingsIban2, paymentIban),
            CreateFrom(baseTransaction, 12, 500, paymentIban, savingsIban2),
            CreateFrom(baseTransaction, 13, -500, savingsIban2, paymentIban),
            // other
            CreateFrom(baseTransaction, 14, -500, paymentIban, storeIban),
        ];

        var db = await fixture.CreateTableClientAsync();

        await db.Transactions.AddRangeAsync(transactions);
        await db.SaveChangesAsync();

        var useCase = new UseCase(db);

        var actual = await useCase.HandleAsync(new Request
        {
            Iban = paymentIban,
            Month = date.Month,
            Year = date.Year
        });

        actual.BalancePerAccount.Should().HaveCount(2);
        actual.BalancePerAccount[savingsIban].Should().Be(-250);
        actual.BalancePerAccount[savingsIban2].Should().Be(-2500);
    }

    [Fact]
    public async Task Previous_month_cashback_should_not_be_in_current_list()
    {
        var date = new DateOnly(2024, 1, 1);
        var payment = new Transaction
        {
            Currency = "EUR",
            Iban = "payment_iban",
            IbanOtherParty = "savings_iban",
            DateTransaction = date,
            FollowNumber = 1,
            Id = 1,
            Amount = -100,
            NameOtherParty = "Savings 1"
        };
        var cashback = new Transaction
        {
            Currency = "EUR",
            Iban = "payment_iban",
            IbanOtherParty = "third_party_iban",
            DateTransaction = date,
            FollowNumber = 2,
            Id = 2,
            Amount = 100,
            NameOtherParty = "friend",
            CashbackForDate = date.AddMonths(-1)
        };

        List<Transaction> transactions = [payment, cashback];
        await using (var db = await fixture.CreateTableClientAsync())
        {
            await db.AddRangeAsync(transactions);
            await db.SaveChangesAsync();
        }

        var result = await new UseCase(await fixture.CreateTableClientAsync(false))
            .HandleAsync(new Request { Month = date.Month, Year = date.Year });

        result.ExpensesVariable.Should().Be(-100);
    }

    private Transaction CreateFrom(Transaction transaction, int id, decimal amount, string fromIban, string toIban,
        DateOnly? cashbackDate = null)
    {
        return new Transaction
        {
            Id = id,
            Amount = amount,
            Iban = fromIban,
            IbanOtherParty = toIban,
            DateTransaction = transaction.DateTransaction,
            FollowNumber = transaction.FollowNumber + id,
            Currency = transaction.Currency,
            NameOtherParty = transaction.NameOtherParty,
            AuthorizationCode = transaction.AuthorizationCode,
            BalanceAfterTransaction = transaction.BalanceAfterTransaction,
            CashbackForDate = cashbackDate ?? transaction.CashbackForDate,
            Description = transaction.Description
        };
    }
}