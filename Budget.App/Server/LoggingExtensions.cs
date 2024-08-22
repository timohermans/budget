
using Serilog;

namespace Budget.App.Server;

public static class LoggingExtensions
{
    /// <summary>
    /// Log with Serilog. Don't forget to remove the default Logging key in appsettings.json.
    /// Also don't forget to add the config. Can be found at https://github.com/serilog/serilog-settings-configuration
    /// </summary>
    /// <param name="services">services from program.cs</param>
    /// <param name="config">appsettings config. See https://github.com/serilog/serilog-aspnetcore/blob/dev/samples/Sample/appsettings.json</param>
    /// <returns>same services from program.cs</returns>
    public static IServiceCollection AddSeriLogLogging(this IServiceCollection services, IConfiguration config)
    {
        services.AddSerilog((configuration) =>
        {
            configuration
                .ReadFrom.Configuration(config)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
        });

        return services;
    }
}