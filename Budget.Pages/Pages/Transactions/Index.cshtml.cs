using Budget.Core.Models;
using Budget.Core.UseCases;
using Budget.Pages.Constants;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;

namespace Budget.Pages.Pages.Transactions;

public class IndexModel(TransactionGetOverviewUseCase useCase, ILogger<IndexModel> logger, IMemoryCache cache) : PageModel
{
    [BindProperty]
    [DisplayName("Rekening")]
    public required string Iban { get; set; }
    public List<SelectListItem> IbansToSelect { get; set; } = [];
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly DatePreviousMonth { get; set; }
    public decimal IncomeLastMonth { get; set; } = 0;
    public decimal ExpensesFixedLastMonth { get; set; } = 0;
    public decimal BudgetAvailable => IncomeLastMonth + ExpensesFixedLastMonth;
    public decimal BudgetPerWeek => WeeksInMonth.Count > 0 ? Math.Floor(BudgetAvailable / WeeksInMonth.Count) : 0;
    public List<int> WeeksInMonth { get; set; } = [];
    public decimal ExpensesVariable { get; set; } = 0;
    public Dictionary<int, decimal> ExpensesPerWeek { get; set; } = [];
    public decimal IncomeFromOwnAccounts { get; set; } = 0;
    public Dictionary<int, List<Transaction>> TransactionsPerWeek { get; set; } = [];

    public void OnGet(int? year, int? month, string? iban)
    {
        var request = new TransactionGetOverviewUseCase.Request
        {
            Year = year ?? DateTime.Now.Year,
            Month = month ?? DateTime.Now.Month,
            Iban = iban
        };
        var cacheKey = CacheKeys.GetTransactionOverviewKey(request);
        TransactionGetOverviewUseCase.Response? result;

        if (!cache.TryGetValue(cacheKey, out result))
        {
            result = useCase.Handle(request);
            cache.Set(cacheKey, result, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });
        }


        Date = result!.Date;
        DatePreviousMonth = result.DatePreviousMonth;
        IncomeLastMonth = result.IncomeLastMonth;
        ExpensesFixedLastMonth = result.ExpensesFixedLastMonth;
        WeeksInMonth = result.WeeksInMonth;
        ExpensesVariable = result.ExpensesVariable;
        ExpensesPerWeek = result.ExpensesPerWeek;
        IncomeFromOwnAccounts = result.IncomeFromOwnAccounts;
        Iban = result.IbanSelected;
        IbansToSelect = result.IbansToSelect.Select(iban => new SelectListItem(iban, iban, iban == Iban)).ToList();
        WeeksInMonth = result.WeeksInMonth;
        TransactionsPerWeek = result.TransactionsPerWeek;

        logger.LogInformation("Load {Iban}'s transactions of {Date}", this.Iban, Date);

        if (Request.IsHtmx())
        {
            Response.Htmx(h => h.WithTrigger(HtmxEvents.UpdateBootstrap, timing: HtmxTriggerTiming.AfterSettle));
        }
    }
}