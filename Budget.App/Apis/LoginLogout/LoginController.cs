using System.Security.Claims;
using Budget.Core.Extensions;
using Budget.Core.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Budget.App.Apis.LoginLogout;

public static class LoginController
{
    public static async Task GetLogin(HttpContext context, ILogger<LoginView> logger)
    {
        logger.LogInformation("Someone is trying to log in.");
        
        await context.ChallengeAsync(new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }

    public static async Task<IResult> PostLogin(
        [FromForm] LoginModel model,
        IValidator<LoginModel> validator,
        LoginThrottler loginThrottler,
        IConfiguration config,
        HttpContext context,
        ILogger<LoginView> logger)
    {
      
        
        var validationResult = await validator.ValidateAsync(model);
        var errors = validationResult.ToDictionary() as Dictionary<string, string[]> ?? new();

        logger.LogInformation("Dictionary after validation: {Errors}", errors);

        if (loginThrottler.GetSecondsLeftToTryAgain() > 0)
        {
            errors.Add("summary", ["Too many login attempts. Wait some time to try again"]);
        }

        var adminUsername = config.GetValue<string>("Admin:Username");
        var adminPassword = config.GetValue<string>("Admin:Password");

        if (errors.Count == 0 && (adminUsername != model.Username || adminPassword != model.Password))
        {
            loginThrottler.Throttle();
            logger.LogWarning(
                "Someone failed login with username {Email} and a given password. Throttler: {SecondsLeft}s",
                model.Username, loginThrottler.GetSecondsLeftToTryAgain());
            errors.Add("summary", ["Username or password is incorrect"]);
        }

        if (errors.Any())
        {
            return new RazorComponentResult<LoginView>(new { Errors = errors });
        }

        logger.LogInformation("Successful login!");
        // Login gebeurt hier
        var claims = new List<Claim>
        {
            new("user", model.Username),
            new("role", "Member")
        };
        await context.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));
        loginThrottler.Reset();

        return Results.Redirect(LoginLogoutApi.TwoFactoryEndpoint);
    }
}