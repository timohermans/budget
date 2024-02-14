using Microsoft.AspNetCore.Authentication;

namespace Budget.App.Apis.LoginLogout;

public static class LogoutController
{
    public static async Task Get(HttpContext context)
    {
        await context.SignOutAsync();
    }
}