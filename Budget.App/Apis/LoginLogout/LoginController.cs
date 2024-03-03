using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Budget.App.Apis.LoginLogout;

public static class LoginController
{
    public static async Task GetLogin(HttpContext context, ILoggerFactory loggerFactory, [FromQuery] string returnUrl)
    {
        var logger = loggerFactory.CreateLogger(nameof(LoginController));
        logger.LogInformation("Someone is trying to log in.");

        await context.ChallengeAsync(new AuthenticationProperties
        {
            RedirectUri = returnUrl
        });
    }
}