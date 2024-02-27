using Microsoft.AspNetCore.Authentication;

namespace Budget.App.Apis.LoginLogout;

public static class LogoutController
{
    public static async Task Get(HttpContext context, IConfiguration config)
    {
        await context.SignOutAsync(new AuthenticationProperties
        {
            RedirectUri = $"{config.GetValue<string>("Auth:Authority")}/protocol/openid-connect/logout"
        });
    }
}