using Budget.Core.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Budget.Pages.Pages.Account;

public class AccessDenied : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        if (!HttpContext.User.IsTwoFactorAuthenticated())
        {
            await HttpContext.SignOutAsync();
            return Redirect("/Account/Login");
        }
        
        return Page();
    }
}