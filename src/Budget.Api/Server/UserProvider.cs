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
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}