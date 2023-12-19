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

namespace Budget.Pages.Pages.Transactions;

public class MarkAsCashbackModel(TableClient table, IMemoryCache cache, ILogger<MarkAsCashbackModel> logger) : PageModel
{
    [BindProperty]
    public required string PartitionKey { get; set; }
    [BindProperty]
    public required string RowKey { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    public required DateTime Date { get; set; } = DateTime.Now;


    public IActionResult OnGet(string? partitionKey, string? rowKey, DateTime? date)
    {
        if (partitionKey == null || rowKey == null)
        {
            return new EmptyResult();
        }

        if (date != null)
        {
            Date = date.Value;
        }

        RowKey = rowKey;
        PartitionKey = partitionKey;

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
        Date = DateTime.SpecifyKind(Date, DateTimeKind.Utc);
        var dateNextMonth = Date.AddMonths(1);
        var transaction = table.Query<Transaction>(t => t.PartitionKey == PartitionKey && t.RowKey == rowKey).FirstOrDefault();

        if (transaction != null)
        {
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new TransactionGetOverviewUseCase.Request { Year = transaction.DateTransaction.Year, Month = transaction.DateTransaction.Month }));
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new TransactionGetOverviewUseCase.Request { Year = Date.Year, Month = Date.Month }));
            cache.Remove(CacheKeys.GetTransactionOverviewKey(new TransactionGetOverviewUseCase.Request { Year = dateNextMonth.Year, Month = dateNextMonth.Month })); // the income needs to be recalculated for the next month

            var transactionUpdated = transaction with
            {
                PartitionKey = Date.ToString("yyyy-MM"),
                CashbackForDate = Date
            };

            table.DeleteEntity(PartitionKey, rowKey);

            try
            {
                table.AddEntity(transactionUpdated);
            } catch (Exception ex)
            {
                logger.LogError(ex, "Error while marking cashback for {Transaction} for date {Date}", transactionUpdated, Date);
                table.AddEntity(transaction);
            }

            logger.LogInformation("Changed {Transaction} from {PartitionKey} to {NewPartitionKey} with cashback date {Date}", transaction.RowKey, PartitionKey, transactionUpdated.PartitionKey, Date);
        }

        return RedirectToPage("Index");
    }
}
