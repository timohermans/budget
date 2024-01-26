using Budget.Core.UseCases;
using Budget.Pages.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using Budget.Core.UseCases.Transactions.Overview;
using UseCase = Budget.Core.UseCases.Transactions.FileEtl.UseCase;

namespace Budget.Pages.Pages.Transactions
{
    public class UploadModel(UseCase useCase, IMemoryCache cache) : PageModel
    {
        public static string TmpAmountInsertedKey = nameof(UploadModel) + "_AmountInserted";
        public static string TmpAmountMinDateKey = nameof(UploadModel) + "_MinDate";
        public static string TmpAmountMaxDateKey = nameof(UploadModel) + "_MaxDate";
        [BindProperty]
        [DisplayName("Rabobank csv bestand")]
        public required IFormFile TransactionsFile { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: filetype validation

            await using var fileStream = TransactionsFile.OpenReadStream();
            var response = await useCase.HandleAsync(fileStream);

            TempData[TmpAmountInsertedKey] = response.AmountInserted;
            TempData[TmpAmountMinDateKey] = response.DateMin.ToShortDateString();
            TempData[TmpAmountMaxDateKey] = response.DateMax.ToShortDateString();

            Enumerable.Range(0, 1 + response.DateMax.Subtract(response.DateMin).Days)
                .Select(offset => response.DateMin.AddDays(offset))
                .Select(date => (date.Year, date.Month))
                .Distinct()
                .ToList()
                .ForEach(pair =>
                {
                    var cacheKey = CacheKeys.GetTransactionOverviewKey(new Request
                    {
                        Year = pair.Year,
                        Month = pair.Month
                    });
                    cache.Remove(cacheKey);
                });


            return RedirectToPage("Index");
        }
    }
}
