namespace Budget.Application.Providers;

public class ManualUserProvider(string username) : IUserProvider
{
    private string? _username = username;

    public string? GetCurrentUser() => _username;

    public void OverrideUser(string username) => _username = username;
}