using Budget.Application.Providers;

namespace Budget.Tests.Utils.Providers;

public class TestUserProvider(string initialUsername) : IUserProvider
{
    private string _username = initialUsername;

    public string? GetCurrentUser() => _username;

    public void OverrideUser(string username) => _username = username;
}