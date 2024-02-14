using Microsoft.AspNetCore.Authentication;

namespace Budget.App.Apis.LoginLogout;

public static class LoginController
{
    public static async Task GetLogin(HttpContext context, ILogger logger)
    {
        logger.LogInformation("Someone is trying to log in.");
        
        await context.ChallengeAsync(new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }
}