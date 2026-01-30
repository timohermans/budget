namespace Budget.Application.Providers;

public interface IUserProvider
{
    string? GetCurrentUser();
    void OverrideUser(string username);
}