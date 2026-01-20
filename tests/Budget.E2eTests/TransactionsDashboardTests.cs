using Budget.Ui.Server.Constants;
using Microsoft.Playwright;

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
public class TransactionsDashboardTests : BaseE2ETests
{
    [TestMethod]
    public async Task Has_budget_as_title()
    {
        var url = AppUrl;

        await Page.GotoAsync(url.ToString());

        await Expect(Page).ToHaveTitleAsync(new Regex("Budget"));
    }
    
    [TestMethod]
    public async Task Shows_current_month_and_can_navigate_to_neighbouring_months()
    {
        var page = new TransactionsDashboardPageObject(Page);
        var url = AppUrl;

        await Page.GotoWithIdleWaitAsync(url.ToString());

        var date = DateTime.Today;
        await Expect(page.Heading).ToContainTextAsync($"{date:yyyy}-{date:MM}");
        await page.NextMonthButton.ClickAsync();
        
        date = date.AddMonths(1);
        await Expect(page.Heading).ToContainTextAsync($"{date:yyyy}-{date:MM}");
        
        await page.PreviousMonthButton.ClickAsync();
        await page.PreviousMonthButton.ClickAsync();
        date = date.AddMonths(-2);
        await Expect(page.Heading).ToContainTextAsync($"{date:yyyy}-{date:MM}");
    }
    
}