using Budget.Application.Providers;

namespace Budget.Tests.Utils.Providers;

public class TestUserProvider : IUserProvider
{
    private readonly string _userName;

    public TestUserProvider(string userName)
    {
        _userName = userName;
    }

    public string? GetCurrentUser() => _userName;
}