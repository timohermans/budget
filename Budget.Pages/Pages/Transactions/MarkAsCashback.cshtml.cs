using Azure.Data.Tables;
using Budget.Core.Models;
using Budget.Core.UseCases;
using Budget.Pages.Constants;
using Htmx;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using Budget.Core.Infrastructure;

namespace Budget.Pages.Pages.Transactions;

public class MarkAsCashbackModel(TransactionMarkAsCashbackUseCase useCase, IMemoryCache cache) : PageModel
{
    [BindProperty]
    public required string PartitionKey { get; set; }
    [BindProperty]
    public required string RowKey { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime? Date { get; set; }


    public IActionResult OnGet(string? partitionKey, string? rowKey, DateTime? date)
    {
        if (partitionKey == null || rowKey == null)
        {
            return new EmptyResult();
        }

        RowKey = rowKey;
        PartitionKey = partitionKey;
        Date = date;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            Response.Htmx(h => h.Retarget($"#cashbackForm{RowKey}").Reswap("outerHTML"));
            return Page();
        }
        var rowKey = RowKey.ToUpper(); // because of lowercase settings in program, the rowkey gets converted to lowercase

        var result = useCase.Handle(new TransactionMarkAsCashbackUseCase.Request(RowKey, PartitionKey, Date));
        Date = DateTime.SpecifyKind(Date ?? result.Data.DateTransaction, DateTimeKind.Utc);
        var dateNextMonth = Date.Value.AddMonths(1);
        
        if (result is SuccessResult<Transaction>)
        {
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new TransactionGetOverviewUseCase.Request { Year = result.Data.DateTransaction.Year, Month = result.Data.DateTransaction.Month }));
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new TransactionGetOverviewUseCase.Request { Year = Date.Value.Year, Month = Date.Value.Month }));
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new TransactionGetOverviewUseCase.Request { Year = dateNextMonth.Year, Month = dateNextMonth.Month })); // the income needs to be recalculated for the next month
        }

        return RedirectToPage("Index");
    }
}
