using Budget.Core.Models;
using Budget.Pages.Constants;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using Budget.Core.Infrastructure;
using Budget.Core.UseCases.Transactions.MarkAsCashback;
using Overview = Budget.Core.UseCases.Transactions.Overview;

namespace Budget.Pages.Pages.Transactions;

public class MarkAsCashbackModel(UseCase useCase, IMemoryCache cache) : PageModel
{
    [BindProperty]
    public required int Id { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    public DateOnly? Date { get; set; }


    public IActionResult OnGet(int? id, DateOnly? date)
    {
        if (!id.HasValue)
        {
            return new EmptyResult();
        }

        Id = id.Value;
        Date = date;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Response.Htmx(h => h.Retarget($"#cashbackForm{Id}").Reswap("outerHTML"));
            return Page();
        }

        var result = await useCase.Handle(new Request(Id, Date));
        Date ??= result.Data.DateTransaction;
        var dateNextMonth = Date.Value.AddMonths(1);
        
        if (result is SuccessResult<Transaction>)
        {
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new Overview.Request { Year = result.Data.DateTransaction.Year, Month = result.Data.DateTransaction.Month, Iban = result.Data.Iban}));
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new Overview.Request { Year = Date.Value.Year, Month = Date.Value.Month }));
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new Overview.Request { Year = dateNextMonth.Year, Month = dateNextMonth.Month })); // the income needs to be recalculated for the next month
        }

        var (redirectYear, redirectMonth, _) = result.Data.OriginalDate;

        return RedirectToPage("./Index", new { Year = redirectYear, Month = redirectMonth, Iban = result.Data.Iban });
    }
}
