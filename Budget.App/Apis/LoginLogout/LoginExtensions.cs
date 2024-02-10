using System.Security.Claims;
using Budget.Core.Constants;

namespace Budget.App.Apis.LoginLogout;

public static class LoginExtensions
{
    public static bool IsTwoFactorAuthenticated(this ClaimsPrincipal user)
    {
        return user.HasClaim(AuthConstants.TwoFactorLoginPolicy, AuthConstants.TwoFactorLoginClaimValue);
    }
}