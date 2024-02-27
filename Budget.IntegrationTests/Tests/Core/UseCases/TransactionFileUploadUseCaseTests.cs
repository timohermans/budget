using Budget.Core.UseCases.Transactions.FileEtl;
using Budget.IntegrationTests.Config;
using Budget.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Budget.IntegrationTests.Tests.Core.UseCases;

[Collection("integration")]
public class TransactionFileUploadUseCaseTests(TestFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public async Task Saves_transactions_successfully_in_the_table()
    {
        // Arrange
        var logger = new XunitLogger<UseCase>(output);
        var client = await fixture.CreateTableClientAsync();
        var useCase = new UseCase(client, logger);

        // Act
        await useCase.HandleAsync(File.OpenRead("Data/transactions-1.csv"));

        // Assert
        var transactions = await client.Transactions.ToListAsync();
        transactions.Should()
            .HaveCount(5)
            .And
            .Satisfy(
                t =>
                    t.Iban == "NL11RABO0104946666"
                    && t.Currency == "EUR"
                    && t.FollowNumber == 12107
                    && t.DateTransaction == new DateOnly(2023, 11, 20)
                    && t.Amount == 4000
                    && t.IbanOtherParty == "NL11INGB00022222"
                    && t.NameOtherParty == "Werkgever 1"
                    && t.IsIncome == true && t.Description == "Salaris 1",
                t => t.Iban == "NL11RABO0104946666"
                     && t.Currency == "EUR"
                     && t.FollowNumber == 12108
                     && t.DateTransaction == new DateOnly(2023, 11, 23)
                     && t.Amount == 2000
                     && t.IbanOtherParty == "NL11INGB00033333"
                     && t.NameOtherParty == "Werkgever 2"
                     && t.IsIncome == true
                     && t.Description == "Salaris 2",
                t => t.Iban == "NL11RABO0104946666"
                     && t.Currency == "EUR"
                     && t.FollowNumber == 12109
                     && t.DateTransaction == new DateOnly(2023, 11, 23)
                     && t.Amount == -800
                     && t.IbanOtherParty == "NL51DEUT0265262461"
                     && t.NameOtherParty == "Kinderopvang"
                     && t.IsFixed == true
                     && t.IsIncome == false
                     && t.Description == "Geld kinderopvang november",
                t => t.Iban == "NL11RABO0104946666"
                     && t.Currency == "EUR"
                     && t.FollowNumber == 12110
                     && t.DateTransaction == new DateOnly(2023, 11, 27)
                     && t.Amount == -1000
                     && t.IbanOtherParty == "NL12INGB00033333"
                     && t.NameOtherParty == "Hypotheek"
                     && t.IsFixed == true
                     && t.IsIncome == false
                     && t.Description == "Hypotheek november 2023",
                t => t.Iban == "NL11RABO0104946666"
                     && t.Currency == "EUR"
                     && t.FollowNumber == 12111
                     && t.DateTransaction == new DateOnly(2023, 12, 2)
                     && t.Amount == -90.75M
                     && t.IbanOtherParty == "NL12INGB00044444"
                     && t.NameOtherParty == "Albert Heijn"
                     && t.IsFixed == false
                     && t.IsIncome == false
                     && t.Description == "Betaalautomaat 2023-11-xx");
    }
}