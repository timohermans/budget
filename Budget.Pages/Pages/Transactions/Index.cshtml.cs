using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Budget.Pages.Pages.Transactions;

public class IndexModel : PageModel
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    public void OnGet(int? year, int? month)
    {
        if (year.HasValue && month.HasValue)
        {
            Date = DateOnly.FromDateTime(new DateTime(year.Value, month.Value, 1));
        }

    }
}