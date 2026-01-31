using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.E2eTests.Helpers;
using Budget.Ui.Server.Constants;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;
using Budget.Ui.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Budget.E2eTests;

public class TransactionsDashboardPageObject(IPage page)
{
    public ILocator Heading => page.GetByTestId(TestIdConstants.HeadingCurrentMonth);
    public ILocator PreviousMonthButton => page.GetByTestId(TestIdConstants.PreviousMonthButton);
    public ILocator NextMonthButton => page.GetByTestId(TestIdConstants.NextMonthButton);
    public ILocator TransactionsTable => page.GetByTestId(TestIdConstants.TransactionsTable);
    public ILocator NoTransactionsMessage => page.GetByTestId(TestIdConstants.NoTransactionsMessage);
}

[TestClass]
public class TransactionsDashboardTests(TestContext testContext) : BaseE2ETests(testContext)
{
    [TestMethod]
    public async Task Has_budget_as_title()
    {
        var page = await _browser.NewPageAsync();
        var url = AppUrl;

        await page.GoToWithAuthenticationAsync(url.ToString(), CreateUniqueUserName("title-checker"));

        await Expect(page).ToHaveTitleAsync(new Regex("Budget"));
    }

    [TestMethod]
    public async Task Shows_current_month_and_can_navigate_to_neighbouring_months()
    {
        var page = await _browser.NewPageAsync();
        var pageObj = new TransactionsDashboardPageObject(page);
        var url = AppUrl;

        await page.GoToWithAuthenticationAsync(url.ToString(), CreateUniqueUserName("month-navigator"));

        var date = DateTime.Today;
        await Expect(pageObj.Heading).ToContainTextAsync($"{date:yyyy}-{date:MM}");
        await pageObj.NextMonthButton.ClickAsync();

        date = date.AddMonths(1);
        await Expect(pageObj.Heading).ToContainTextAsync($"{date:yyyy}-{date:MM}");

        await pageObj.PreviousMonthButton.ClickAsync();
        await pageObj.PreviousMonthButton.ClickAsync();
        date = date.AddMonths(-2);
        await Expect(pageObj.Heading).ToContainTextAsync($"{date:yyyy}-{date:MM}");
    }

    [TestMethod]
    public async Task Uploads_transactions_file_and_displays_transactions()
    {
        // Arrange
        var username = CreateUniqueUserName("file-uploader");
        var page = await _browser.NewPageAsync();
        var pageObj = new TransactionsDashboardPageObject(page);

        await page.GoToWithAuthenticationAsync(AppUrl.ToString(), username);

        var fixture = new TransactionsFileCsvMap
        {
            Iban = "NL01RABO0123456789",
            Amount = 123.45m,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Description1 = "Test Transaction Upload",
            Currency = "EUR",
            FollowNumber = 1,
            BalanceAfter = 1000m,
            IbanOtherParty = "NL02RABO9876543210",
            NameOtherParty = "Other Party",
            Code = "GT"
        };
        var paypal1 = new TransactionsFileCsvMap
        {
            Iban = "NL01RABO0123456789",
            Currency = "EUR",
            FollowNumber = 1289,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Amount = -42.34m,
            BalanceAfter = 349.74m,
            IbanOtherParty = "LU89751001135104201E",
            NameOtherParty = "PayPal Europe S.a.r.l. et Cie S.C.A",
            Code = "ei",
            Description1 = "1045309341372/PAYPAL"
        };

        var paypal2 = new TransactionsFileCsvMap
        {
            Iban = "NL01RABO0123456789",
            Currency = "EUR",
            FollowNumber = 14927,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Amount = -9.99m,
            BalanceAfter = 328.11m,
            IbanOtherParty = "LU89751000115104201E",
            NameOtherParty = "PayPal Europe S.a.r.l. et Cie S.C.A",
            Code = "ei",
            Description1 = "1045338748320/PAYPAL"
        };

        await using var csvBuilder = new TransactionsCsvFileBuilder();
        var filePath = await csvBuilder.AddRecords(
                fixture,
                paypal1,
                paypal2)
            .BuildAsync();

        // Act
        var fileInput = page.GetByTestId(TestIdConstants.UploadTransactionsButton);
        await fileInput.SetInputFilesAsync(filePath);

        await Expect(page.GetByTestId(TestIdConstants.UploadTransactionsButtonLoader)).ToBeVisibleAsync();

        await Expect(pageObj.Heading).ToContainTextAsync($"{fixture.Date:yyyy}-{fixture.Date:MM}");

        await using var db = await CreateContext(username);

        // Assert first row
        var transaction = await db.Transactions.SingleAsync(t => t.Description == fixture.Description1);
        var row = page.GetByTestId($"transaction-row-{transaction.Id}");
        await Expect(row).ToBeVisibleAsync();
        await Expect(row.GetByTestId("transaction-week-number"))
            .ToContainTextAsync(transaction.DateTransaction.ToIsoWeekNumber().ToString());
        await Expect(row.GetByTestId("transaction-date")).ToContainTextAsync(fixture.Date.ToString("dd-MM"));
        await Expect(row.GetByTestId("transaction-name")).ToContainTextAsync(fixture.NameOtherParty!);
        await Expect(row.GetByTestId("transaction-amount")).ToContainTextAsync(fixture.Amount?.ToString() ?? "");
        await Expect(row.GetByTestId("transaction-fixed-status")).ToHaveTextAsync("");

        // Assert paypal1
        var transactionPayPal1 = await db.Transactions.SingleAsync(t => t.Description == paypal1.Description1);
        var rowPayPal1 = page.GetByTestId($"transaction-row-{transactionPayPal1.Id}");
        await Expect(rowPayPal1).ToBeVisibleAsync();
        await Expect(rowPayPal1.GetByTestId("transaction-week-number"))
            .ToContainTextAsync(transactionPayPal1.DateTransaction.ToIsoWeekNumber().ToString());
        await Expect(rowPayPal1.GetByTestId("transaction-date")).ToContainTextAsync(paypal1.Date.ToString("dd-MM"));
        await Expect(rowPayPal1.GetByTestId("transaction-name")).ToContainTextAsync(paypal1.NameOtherParty!);
        await Expect(rowPayPal1.GetByTestId("transaction-amount")).ToContainTextAsync(paypal1.Amount?.ToString() ?? "");
        await Expect(rowPayPal1.GetByTestId("transaction-fixed-status")).ToBeEmptyAsync();

        // Assert paypal2
        var transactionPayPal2 = await db.Transactions.SingleAsync(t => t.Description == paypal2.Description1);
        var rowPayPal2 = page.GetByTestId($"transaction-row-{transactionPayPal2.Id}");
        await Expect(rowPayPal2).ToBeVisibleAsync();
        await Expect(rowPayPal2.GetByTestId("transaction-week-number"))
            .ToContainTextAsync(transactionPayPal2.DateTransaction.ToIsoWeekNumber().ToString());
        await Expect(rowPayPal2.GetByTestId("transaction-date")).ToContainTextAsync(paypal2.Date.ToString("dd-MM"));
        await Expect(rowPayPal2.GetByTestId("transaction-name")).ToContainTextAsync(paypal2.NameOtherParty!);
        await Expect(rowPayPal2.GetByTestId("transaction-amount")).ToContainTextAsync(paypal2.Amount?.ToString() ?? "");
        await Expect(rowPayPal2.GetByTestId("transaction-fixed-status")).ToBeEmptyAsync();
    }
}