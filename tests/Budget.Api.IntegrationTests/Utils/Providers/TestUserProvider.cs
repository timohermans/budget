using Budget.Application.Providers;

namespace Budget.Api.IntegrationTests.Utils.Providers;

public class TestUserProvider : IUserProvider
{
    private readonly string _userName;

    public TestUserProvider(string userName)
    {
        _userName = userName;
    }

    public string? GetCurrentUser() => _userName;
}