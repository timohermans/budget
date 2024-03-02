using Microsoft.EntityFrameworkCore;

namespace Budget.BlazorTests.Tests;

[TestFixture]
internal class TransactionFileUploadTests : BlazorTest
{
    [Test]
    public async Task UploadsAFileWithTransactionsSuccessfully()
    {
        await GotoAsync("transactions");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Upload transactions" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).SetInputFilesAsync(new[] { "Data/transactions-1.csv" });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Upload transactions" })).ToBeVisibleAsync();
        await Page.GotoAsync("http://localhost:5223/transactions?year=2023&month=11");

        await Expect(Page.GetByRole(AriaRole.Row)).ToHaveCountAsync(5);

        await Page.GetByTestId("goNext").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Row)).ToHaveCountAsync(2);

        // Assertions below is a leftover from integration testing
        // Since overview has been tested with the Overview test,
        // I actually prefer to keep it this way :).
        await using var db = DatabaseHelper.CreateDbContextAsync();
        var transactions = await db.Transactions.ToListAsync();
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
