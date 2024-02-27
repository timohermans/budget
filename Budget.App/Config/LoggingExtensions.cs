using Serilog;

namespace Budget.App.Config;

public static class LoggingExtensions
{
    public static void AddLogging(this ConfigureHostBuilder host, IConfiguration config)
    {
        host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console();
        });
    }
}