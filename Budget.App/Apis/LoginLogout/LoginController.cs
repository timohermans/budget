using Microsoft.AspNetCore.Authentication;

namespace Budget.App.Apis.LoginLogout;

public static class LoginController
{
    public static async Task GetLogin(HttpContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(LoginController));
        logger.LogInformation("Someone is trying to log in.");
        
        await context.ChallengeAsync(new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }
}