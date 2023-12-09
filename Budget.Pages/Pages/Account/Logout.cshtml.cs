using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Budget.Pages.Pages.Account;

public class Logout : PageModel
{
    public async Task<IActionResult> OnPost()
    {
        await HttpContext.SignOutAsync();
        return RedirectToPage("/Account/Login");
    }
}