using Budget.Core.Models;

namespace Budget.BlazorTests.Tests;

[TestFixture]
internal class TransactionOverviewTests : BlazorWithTraceTest
{
    [Test]
    public async Task TransactionOverviewShowsAggregatedAndListData()
    {
        var paymentIban = "NL22OWN01010100";
        var savingsIban = "NL33PERSONAL01010100";
        var savingsIban2 = "NL33SPAAR01010101";
        var otherIban = "NL55OTHER01010100";
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
            CreateFrom(baseTransaction, -1, new DateOnly(2024, 1, 23), 1000, "Salaris BF", paymentIban, otherIban),
            CreateFrom(baseTransaction, -2, new DateOnly(2024, 1, 15), 1500, "Salaris GF", paymentIban, otherIban),
            CreateFrom(baseTransaction, -3, new DateOnly(2024, 1, 15), -1500, "Vaste lasten!", paymentIban, otherIban, authorizationCode: "vast"),

            CreateFrom(baseTransaction, 2, new DateOnly(2024, 2, 11), -100, "Geld tekort oordopjes", paymentIban, savingsIban),
            CreateFrom(baseTransaction, 3, new DateOnly(2024, 2, 11), 100, "Geld tekort oordopjes", savingsIban, paymentIban),
            CreateFrom(baseTransaction, 4, new DateOnly(2024, 2, 11), -200, "APK auto", paymentIban, savingsIban),
            CreateFrom(baseTransaction, 5, new DateOnly(2024, 2, 11), 200, "APK auto", savingsIban, paymentIban),
            CreateFrom(baseTransaction, 6, new DateOnly(2024, 2, 11), 50, "Etentje voorschieten", paymentIban, savingsIban),
            CreateFrom(baseTransaction, 7, new DateOnly(2024, 2, 11), -50, "Etentje voorschieten", savingsIban, paymentIban),
            // savings2
            CreateFrom(baseTransaction, 8, new DateOnly(2024, 2, 1), -1000, "Stichting Hypotheken Incasso", paymentIban, savingsIban2),
            CreateFrom(baseTransaction, 9, new DateOnly(2024, 2, 1), 1000, "Stichting Hypotheken Incasso", savingsIban2, paymentIban),
            CreateFrom(baseTransaction, 10, new DateOnly(2024, 2, 11), -2000, "Sparen BF", paymentIban, savingsIban2),
            CreateFrom(baseTransaction, 11, new DateOnly(2024, 2, 11), 2000, "Sparen BF", savingsIban2, paymentIban),
            CreateFrom(baseTransaction, 12, new DateOnly(2024, 2, 11), 500, "Sparen CH", paymentIban, savingsIban2),
            CreateFrom(baseTransaction, 13, new DateOnly(2024, 2, 11), -500, "Sparen CH", savingsIban2, paymentIban),
            // other
            CreateFrom(baseTransaction, 21, new DateOnly(2024, 2, 1), -38.55M, "Zalando Payments GmbH", paymentIban, otherIban),
            CreateFrom(baseTransaction, 22, new DateOnly(2024, 2, 1), -5.12M, "Levensverzekering", paymentIban, otherIban),
            CreateFrom(baseTransaction, 23, new DateOnly(2024, 2, 8), -18, "Social deal", paymentIban, otherIban),
            CreateFrom(baseTransaction, 24, new DateOnly(2024, 2, 8), -58, "Jumbo Amsterdam Stadion1", paymentIban, otherIban),
            CreateFrom(baseTransaction, 25, new DateOnly(2024, 2, 13), -2, "Ballorig Amsterdam", paymentIban, otherIban),
            CreateFrom(baseTransaction, 26, new DateOnly(2024, 2, 18), -15, "LUNA", paymentIban, otherIban),
            CreateFrom(baseTransaction, 27, new DateOnly(2024, 2, 23), -54, "ROOS SCHADEVERZ NV", paymentIban, otherIban),
            CreateFrom(baseTransaction, 28, new DateOnly(2024, 2, 23), -112.90M, "AH- Jan Linders 4222", paymentIban, otherIban),
            CreateFrom(baseTransaction, 14, new DateOnly(2024, 2, 27), -200, "APK auto", paymentIban, otherIban),
            CreateFrom(baseTransaction, 15, new DateOnly(2024, 2, 27), -20, "SIMYO", paymentIban, otherIban),
            CreateFrom(baseTransaction, 16, new DateOnly(2024, 2, 27), -18, "SpotifyAB", paymentIban, otherIban),
            CreateFrom(baseTransaction, 17, new DateOnly(2024, 2, 27), -280, "CZ ZORGVERZEKERINGEN NV", paymentIban, otherIban),
            CreateFrom(baseTransaction, 18, new DateOnly(2024, 2, 27), -300, "MOK Kinderopvang B.V.", paymentIban, otherIban),
            CreateFrom(baseTransaction, 19, new DateOnly(2024, 2, 27), -16, "NV WATERLEIDING MAATSCHAPPIJ LIMBURG", paymentIban, otherIban),
            CreateFrom(baseTransaction, 20, new DateOnly(2024, 2, 27), -112, "AH- Jan Linders 4181", paymentIban, otherIban),
        ];

        await using var db = DatabaseHelper.CreateDbContextAsync();

        await db.Transactions.AddRangeAsync(transactions);
        await db.SaveChangesAsync();

        await GotoAsync($"transactions?year=2024&month=2");

        await Expect(Page.GetByRole(AriaRole.Navigation)).ToContainTextAsync("2024-02");
        await Expect(Page.GetByRole(AriaRole.Navigation)).ToContainTextAsync("timo");
        await Expect(Page.GetByRole(AriaRole.Combobox)).ToHaveValueAsync(paymentIban);

        await Expect(Page.GetByTestId("previousMonthHeader")).ToContainTextAsync("januari");
        await Expect(Page.GetByTestId("previousMonthIncome")).ToContainTextAsync("2500,00");
        await Expect(Page.GetByTestId("previousMonthExpensesFixed")).ToContainTextAsync("-1500,00");
        await Expect(Page.GetByTestId("budgetAvailable")).ToContainTextAsync("1000,00");
        await Expect(Page.GetByTestId("budgetCalculationMonthHeader")).ToContainTextAsync("februari");
        await Expect(Page.GetByTestId("budgetCalculationWeeksCount")).ToContainTextAsync("5");
        await Expect(Page.GetByTestId("budgetCalculationBudgetPerWeek")).ToContainTextAsync("200");

        await Expect(Page.GetByTestId("expensesMonthHeader")).ToContainTextAsync("februari");
        await Expect(Page.GetByTestId("expensesBudgetAvailable")).ToContainTextAsync("1000,00");
        await Expect(Page.GetByTestId("expensesWeek5Spent")).ToContainTextAsync("-43,67");
        await Expect(Page.GetByTestId("expensesWeek5BudgetLeft")).ToContainTextAsync("156,33");
        await Expect(Page.GetByTestId("expensesWeek6Spent")).ToContainTextAsync("-76,00");
        await Expect(Page.GetByTestId("expensesWeek6BudgetLeft")).ToContainTextAsync("124,00");
        await Expect(Page.GetByTestId("expensesWeek7Spent")).ToContainTextAsync("-17,00");
        await Expect(Page.GetByTestId("expensesWeek7BudgetLeft")).ToContainTextAsync("183,00");
        await Expect(Page.GetByTestId("expensesWeek8Spent")).ToContainTextAsync("-166,90");
        await Expect(Page.GetByTestId("expensesWeek8BudgetLeft")).ToContainTextAsync("33,10");
        await Expect(Page.GetByTestId("expensesWeek9Spent")).ToContainTextAsync("-946,00");
        await Expect(Page.GetByTestId("expensesWeek9BudgetLeft")).ToContainTextAsync("-746,00");

        await Expect(Page.GetByTestId("totalsSpent")).ToContainTextAsync("-1249,57");
        await Expect(Page.GetByTestId("totalsLeft")).ToContainTextAsync("-249,57");
        await Expect(Page.GetByTestId("totalsSaved")).ToContainTextAsync("2750,00");
        await Page.GetByText("Spaarmeter").ClickAsync();
        await Expect(Page.GetByTestId("savedNL33SPAAR01010101")).ToContainTextAsync("2500,00");
        await Expect(Page.GetByTestId("savedNL33PERSONAL01010100")).ToContainTextAsync("250,00");

        await Expect(Page.GetByRole(AriaRole.Row)).ToHaveCountAsync(22);

        await Page.GetByTestId("goPrevious").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Row)).ToHaveCountAsync(4);

        await Page.GetByTestId("goNext").ClickAsync();
        await Page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "NL33SPAAR01010101" });

        await Expect(Page.GetByTestId("previousMonthIncome")).ToContainTextAsync("0");
        await Expect(Page.GetByRole(AriaRole.Row)).ToHaveCountAsync(4);
    }

    private Transaction CreateFrom(Transaction transaction, int id, DateOnly date, decimal amount, string nameOtherParty, string fromIban, string toIban,
    DateOnly? cashbackDate = null, string? authorizationCode = null)
    {
        return new Transaction
        {
            Amount = amount,
            Iban = fromIban,
            IbanOtherParty = toIban,
            DateTransaction = date,
            FollowNumber = transaction.FollowNumber + id,
            Currency = transaction.Currency,
            NameOtherParty = nameOtherParty,
            AuthorizationCode = authorizationCode,
            BalanceAfterTransaction = transaction.BalanceAfterTransaction,
            CashbackForDate = cashbackDate ?? transaction.CashbackForDate,
            Description = transaction.Description,
        };
    }
}
