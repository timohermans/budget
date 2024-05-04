using Budget.Core.UseCases.Transactions.Overview;

namespace Budget.Htmx.Config;

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(_ => TimeProvider.System);
        services.AddHttpClient();

        typeof(OverviewUseCase)
            .Assembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("UseCase"))
            .ToList()
            .ForEach(t => services.AddScoped(t));

        return services;
    }
}