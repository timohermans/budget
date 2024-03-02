using Budget.Core.Models;

namespace Budget.BlazorTests.Tests;

[TestFixture]
internal class TransactionMarkAsCashbackTests : BlazorTest
{
    [Test]
    public async Task MarksAndUnMarksAnIncomeAsCashback()
    {
        List<Transaction> transactions = [
            new Transaction {
                FollowNumber = 1,
                Currency = "EUR",
                Iban = "NL11Betaal rekening",
                Amount = -1,
                NameOtherParty = "Snoep automaat",
                IbanOtherParty = "NL66Bedrijf",
                DateTransaction = new DateOnly(2024, 2, 1),
            },
           new Transaction {
                FollowNumber = 2,
                Currency = "EUR",
                Iban = "NL11Betaal rekening",
                Amount = 10,
                NameOtherParty = "Mediaan",
                IbanOtherParty = "NL77EBedrijf",
                DateTransaction = new DateOnly(2024, 2, 1),
            } ,
           new Transaction {
                FollowNumber = 3,
                Currency = "EUR",
                Iban = "NL11Betaal rekening",
                Amount = 1,
                NameOtherParty = "S.O.M. Graveyard eo J.E.H.",
                Description = "Terugbetaling snoepje",
                IbanOtherParty = "NL22Broertje",
                DateTransaction = new DateOnly(2024, 2, 1),
            }
       ];

        await using var db = DatabaseHelper.CreateDbContextAsync();
        await db.Transactions.AddRangeAsync(transactions);
        await db.SaveChangesAsync();

        await GotoAsync("transactions?year=2024&month=2");

        // cancelling the cashback marking
        await Page.GetByRole(AriaRole.Row, new() { Name = "S.O.M. Graveyard eo" }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Als terugbetaling markeren" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel marking" }).ClickAsync();
        await Expect(Page.GetByLabel("S.O.M. Graveyard eo J.E.H.")).Not.ToBeVisibleAsync();

        // persisting the cashback marking
        await Page.GetByRole(AriaRole.Row, new() { Name = "S.O.M. Graveyard eo" }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Als terugbetaling markeren" }).ClickAsync();
        await Page.GetByLabel("S.O.M. Graveyard eo J.E.H.").FillAsync("2024-02-02");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save cashback date" }).ClickAsync();

        // making sure the tooltip is there
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Originele datum: 2024-02-01" })).ToBeVisibleAsync();

        // unmarking the cashback marking
        await Page.GetByRole(AriaRole.Row, new() { Name = "S.O.M. Graveyard eo" }).GetByRole(AriaRole.Button).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Als terugbetaling markeren" }).ClickAsync();
        await Page.GetByLabel("S.O.M. Graveyard eo J.E.H.").FillAsync("");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save cashback date" }).ClickAsync();
        await Expect(Page.GetByLabel("S.O.M. Graveyard eo J.E.H.")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Originele datum: 2024-02-01" })).Not.ToBeVisibleAsync();
    }
}
