using System.Security.Claims;
using Budget.Core.Constants;

namespace Budget.Core.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static bool IsFullyAuthenticated(this ClaimsPrincipal user)
    {
        return (user.Identity?.IsAuthenticated ?? false) && user.IsTwoFactorAuthenticated();
    }

    public static bool IsTwoFactorAuthenticated(this ClaimsPrincipal user)
    {
        return user.HasClaim(AuthConstants.TwoFactorLoginPolicy, AuthConstants.TwoFactorLoginClaimValue);
    }
}