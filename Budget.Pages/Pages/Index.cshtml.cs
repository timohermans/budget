using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Budget.Pages.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    public IActionResult OnGet()
    {
        _logger.LogInformation("Moving away from home");
        return RedirectToPage("/Transactions/Index");
    }
}