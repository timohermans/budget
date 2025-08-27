namespace Budget.Ui.Server;

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(_ => TimeProvider.System);

        return services;
    }
}