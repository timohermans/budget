using Microsoft.AspNetCore.Authentication;

namespace Budget.App.Apis.LoginLogout;

public static class LogoutController
{
    public static async Task<IResult> Post(HttpContext context, CancellationToken token)
    {
        await context.SignOutAsync();
        return Results.Redirect(LoginLogoutApi.LoginEndpoint);
    }
}