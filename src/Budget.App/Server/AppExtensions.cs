using Budget.App.States;
using Budget.Core.UseCases.Transactions.Overview;

namespace Budget.App.Server;

public static class AppExtensions
{
    public static IServiceCollection AddBudgetServices(this IServiceCollection services)
    {
        services.AddSingleton(_ => TimeProvider.System);
        typeof(OverviewUseCase)
            .Assembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("UseCase"))
            .ToList()
            .ForEach(t => services.AddScoped(t));

        services.AddScoped<TransactionFilterState>();

        return services;
    }
}
