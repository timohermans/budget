using Budget.Application.Providers;

namespace Budget.MigrationsRunner.Providers;

public class DummyUserProvider : IUserProvider
{
    public string? GetCurrentUser() => "migrations-runner";
}