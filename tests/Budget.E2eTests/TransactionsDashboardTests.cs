using System.Globalization;
using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Ui.Server.Constants;
using CsvHelper;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

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
        var username = CreateUniqueUserName("file-uploader");
        var page = await _browser.NewPageAsync();
        var pageObj = new TransactionsDashboardPageObject(page);

        await page.GoToWithAuthenticationAsync(AppUrl.ToString(), username);

        // Create CSV content
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

        var fileName = $"transactions-{Guid.NewGuid()}.csv";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);

        try
        {
            await using (var writer = new StreamWriter(filePath))
            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(new[] { fixture });
            }

            // Upload file
            var fileInput = page.GetByTestId(TestIdConstants.UploadTransactionsButton);
            await fileInput.SetInputFilesAsync(filePath);

            await Expect(pageObj.Heading).ToContainTextAsync($"{fixture.Date:yyyy}-{fixture.Date:MM}");
            // Verify transaction appears in the list (assuming description is shown)
            // We use a locator that looks for the row containing the description
            await Expect(page.GetByText("Test Transaction Upload")).ToBeVisibleAsync();
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}