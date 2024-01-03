using System.Security.Claims;
using Budget.Core.Infrastructure;
using Budget.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Budget.Pages.Pages.Account;

public class Login : PageModel
{
    private readonly ILogger<Login> _logger;
    private readonly IConfiguration _config;
    private readonly LoginThrottler _loginThrottler;
    
    public Login(ILogger<Login> logger, IConfiguration config, LoginThrottler throttler)
    {
        _logger = logger;
        _config = config;
        _loginThrottler = throttler;
    }

    [BindProperty] public new required User User { get; set; }

    public void OnGet()
    {
        _logger.LogInformation("Someone is trying to log in.");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = "/")
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (_loginThrottler.GetSecondsLeftToTryAgain() > 0)
        {
            ModelState.AddModelError("throttled", "Too many login attempts. Wait some time to try again");
            return Page();
        }

        var adminUsername = _config.GetValue<string>("Admin:Username");
        var adminPassword = _config.GetValue<string>("Admin:Password");

        if (adminUsername != User.Username || adminPassword != User.Password)
        {
            _loginThrottler.Throttle();
            _logger.LogWarning("Someone failed login with username {Email} and a given password. Throttler: {SecondsLeft}s", User.Username, _loginThrottler.GetSecondsLeftToTryAgain());
            ModelState.AddModelError("login", "Username or password is incorrect");
            return Page();
        }

        _logger.LogInformation("Successful login!");
        // Login gebeurt hier
        var claims = new List<Claim>
        {
            new("user", adminUsername),
            new("role", "Member")
        };
        await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));
        _loginThrottler.Reset();

        if (Url.IsLocalUrl(returnUrl))
        {
            return RedirectToPage("TwoFactorLogin", null, routeValues: new { returnUrl });
        }

        return RedirectToPage("TwoFactorLogin");
    }
}