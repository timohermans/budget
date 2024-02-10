using System.Security.Claims;
using Budget.Core.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OtpNet;

namespace Budget.App.Apis.LoginLogout;

public static class TwoFactorController
{
    public static IResult Get(
        ILogger<TwoFactorView> logger,
        HttpContext context)
    {
        logger.LogDebug("Someone is trying to 2 factor");
        logger.LogDebug(string.Join(", ", context.User.Claims.Select(c => c.Value).ToList()));
        
        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            return Results.Redirect(LoginLogoutApi.LoginEndpoint);
        }

        if (context.User.IsTwoFactorAuthenticated())
        {
            return Results.Redirect("/");
        }
        return new RazorComponentResult<TwoFactorView>();
    }

    public static async Task<IResult> Post(
        [FromForm] string code,
        HttpContext context,
        IConfiguration config,
        ILogger<TwoFactorView> logger)
    {
        string? error = null;
        
        if (!(context.User.Identity?.IsAuthenticated ?? false))
        {
            return Results.Redirect(LoginLogoutApi.LoginEndpoint);
        }
        
        if (string.IsNullOrEmpty(code))
        {
            error = "Code is vereist";
        }

        if (!string.IsNullOrEmpty(error))
        {
            return new RazorComponentResult<TwoFactorView>(new { Error = error });
        }
        
        var secret = config.GetValue<string>("Admin:TwoFactorSecret");
        if (secret == null)
        {
            throw new NullReferenceException(
                "Two factor secret missing. Generate one with KeyGenerator.GenerateRandomKey(20) and Base32Encoding.ToString and put it in config");
        }

        var base32Bytes = Base32Encoding.ToBytes(secret);
        var otp = new Totp(base32Bytes);
        if (!otp.VerifyTotp(code, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
        {
            logger.LogWarning("2FA failure");
            error = "One time password was incorrect";
            return new RazorComponentResult<TwoFactorView>(new { Error = error });
        }

        logger.LogInformation($"2FA success. Time step match: {timeStepMatched}");
        ((ClaimsIdentity)context.User.Identity).AddClaim(new Claim(AuthConstants.TwoFactorLoginPolicy,
            AuthConstants.TwoFactorLoginClaimValue));
        await context.SignInAsync(context.User);

        return Results.Redirect("/");
    }
}