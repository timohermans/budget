using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Htmx.Endpoints.LoginLogout;

public static class LoginController
{
    public static IResult GetLogin(HttpContext context, ILoggerFactory loggerFactory, [FromQuery] string? returnUrl)
    {
        var logger = loggerFactory.CreateLogger(nameof(LoginController));
        logger.LogInformation("Someone is trying to log in.");

        return Results.Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        });
    }
}