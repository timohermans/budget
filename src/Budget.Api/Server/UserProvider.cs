using System.Security.Claims;
using Budget.Application.Providers;

namespace Budget.Api.Server;

public class UserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            return null;
        }

        return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
               ?? _httpContextAccessor.HttpContext?.User.Identity?.Name
               ?? user.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
    }

    public void OverrideUser(string username)
    {
        throw new InvalidOperationException($"I'm sorry, I'm not allowing you to change the name to {username}");
    }
}