namespace Budget.Application.Providers;

public class StaticUserProvider(string userName) : IUserProvider
{
    public string? GetCurrentUser() => userName;
}