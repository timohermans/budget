using Serilog;

namespace Budget.Htmx.Config;

public static class LoggingExtensions
{
    public static IHostBuilder AddAppLogging(this IHostBuilder host)
    {
        host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console();
        });

        return host;
    }
}