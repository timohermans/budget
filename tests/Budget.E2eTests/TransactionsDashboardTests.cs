using Budget.Ui.Server.Constants;
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

        await page.GotoWithIdleWaitAsync(url.ToString());
        var user = CreateUniqueUserName("title");
        await AuthenticateUserAsync(page, user);

        await Expect(page).ToHaveTitleAsync(new Regex("Budget"));
    }

    [TestMethod]
    public async Task Shows_current_month_and_can_navigate_to_neighbouring_months()
    {
        var page = await _browser.NewPageAsync();
        var pageObj = new TransactionsDashboardPageObject(page);
        var url = AppUrl;

        await page.GotoWithIdleWaitAsync(url.ToString());
        var user = CreateUniqueUserName("navigation");
        await AuthenticateUserAsync(page, user);

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
}