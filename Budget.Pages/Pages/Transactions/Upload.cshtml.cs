using Azure.Data.Tables;
using Budget.Core.Models;
using Budget.Core.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;

namespace Budget.Pages.Pages.Transactions
{
    public class UploadModel(TransactionFileUploadUseCase useCase) : PageModel
    {
        public static string TmpAmountInsertedKey = nameof(UploadModel) + "_AmountInserted";
        public static string TmpAmountMinDateKey = nameof(UploadModel) + "_MinDate";
        public static string TmpAmountMaxDateKey = nameof(UploadModel) + "_MaxDate";
        [BindProperty]
        [DisplayName("Rabobank csv bestand")]
        public required IFormFile TransactionsFile { get; set; }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: filetype validation

            using var fileStream = TransactionsFile.OpenReadStream();
            var response = useCase.Handle(fileStream);

            TempData[TmpAmountInsertedKey] = response.AmountInserted;
            TempData[TmpAmountMinDateKey] = response.DateMin.ToShortDateString();
            TempData[TmpAmountMaxDateKey] = response.DateMax.ToShortDateString();

            return RedirectToPage("Index");
        }
    }
}
