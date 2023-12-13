using Azure.Data.Tables;
using Budget.Core.Models;
using Budget.Core.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace Budget.Pages.Pages.Transactions
{
    public class UploadModel(TableClient db, ILogger<UploadModel> logger) : PageModel
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
            var usecase = new TransactionFileUploadUseCase(db, logger);

            // TODO: filetype validation

            using var fileStream = TransactionsFile.OpenReadStream();
            var response = await usecase.HandleAsync(fileStream);

            TempData[TmpAmountInsertedKey] = response.AmountInserted;
            TempData[TmpAmountMinDateKey] = response.DateMin.ToShortDateString();
            TempData[TmpAmountMaxDateKey] = response.DateMax.ToShortDateString();

            return RedirectToPage("Index");
        }
    }
}
