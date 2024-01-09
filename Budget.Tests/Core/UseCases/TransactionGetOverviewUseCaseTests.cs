using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using Budget.Core.Models;
using Budget.Core.UseCases;
using Budget.Tests.Helpers;
using FakeItEasy;

namespace Budget.Tests.Core.UseCases;

public class TransactionGetOverviewUseCaseTests
{
    [Fact]
    public void Keeps_check_of_how_much_each_own_account_saved()
    {
        var paymentIban = "NL22OWN01010100";
        var savingsIban = "NL33OWN01010100";
        var savingsIban2 = "NL44OWN01010100";
        var storeIban = "NL55OTHER01010100";
        var date = new DateTime(2023, 12, 1);
        var partitionKey = Transaction.CreatePartitionKey(date);
        var baseTransaction = new Transaction
        {
            Currency = "EUR",
            Iban = paymentIban,
            IbanOtherParty = savingsIban,
            DateTransaction = date,
            FollowNumber = 1,
            PartitionKey = partitionKey,
            RowKey = "1",
            Amount = -100,
            NameOtherParty = "Savings 1"
        };
        IEnumerable<Transaction> transactions =
        [
            baseTransaction with { RowKey = "2", Amount = -100, Iban = paymentIban, IbanOtherParty = savingsIban },
            baseTransaction with { RowKey = "3", Amount = 100, Iban = savingsIban, IbanOtherParty = paymentIban },
            baseTransaction with { RowKey = "4", Amount = -200, Iban = paymentIban, IbanOtherParty = savingsIban },
            baseTransaction with { RowKey = "5", Amount = 200, Iban = savingsIban, IbanOtherParty = paymentIban },
            baseTransaction with { RowKey = "6", Amount = 50, Iban = paymentIban, IbanOtherParty = savingsIban },
            baseTransaction with { RowKey = "7", Amount = -50, Iban = savingsIban, IbanOtherParty = paymentIban },
            // savings2
            baseTransaction with { RowKey = "8", Amount = -1000, Iban = paymentIban, IbanOtherParty = savingsIban2 },
            baseTransaction with { RowKey = "9", Amount = 1000, Iban = savingsIban2, IbanOtherParty = paymentIban },
            baseTransaction with { RowKey = "10", Amount = -2000, Iban = paymentIban, IbanOtherParty = savingsIban2 },
            baseTransaction with { RowKey = "11", Amount = 2000, Iban = savingsIban2, IbanOtherParty = paymentIban },
            baseTransaction with { RowKey = "12", Amount = 500, Iban = paymentIban, IbanOtherParty = savingsIban2 },
            baseTransaction with { RowKey = "13", Amount = -500, Iban = savingsIban2, IbanOtherParty = paymentIban },
            // other
            baseTransaction with { RowKey = "13", Amount = -500, Iban = paymentIban, IbanOtherParty = storeIban },
        ];
        
        var tableClientMock = A.Fake<TableClient>();
        A.CallTo(() => tableClientMock.Query(A<Expression<Func<Transaction, bool>>>._, A<int?>._, A<IEnumerable<string>>._, A<CancellationToken>._))
            .Returns(transactions.ToPageable());

        var useCase = new TransactionGetOverviewUseCase(tableClientMock);

        var actual = useCase.Handle(new TransactionGetOverviewUseCase.Request
        {
            Iban = paymentIban,
            Month = date.Month,
            Year = date.Year
        });

        actual.BalancePerAccount.Should().HaveCount(2);
        actual.BalancePerAccount[savingsIban].Should().Be(-250);
        actual.BalancePerAccount[savingsIban2].Should().Be(-2500);
    }

}