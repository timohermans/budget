using Budget.App;
using Budget.App.Config;
using Serilog;

public class Program
{
    private static async Task Main(string[] args)
    {
        var host = BuildWebHost(args).Build();
        Log.Information("Starting application");

        await host.RunAsync();
    }

    public static IHostBuilder BuildWebHost(string[]? args = null)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Warning("args: {args}", args);

        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStaticWebAssets();
                webBuilder.UseStartup<Startup>();
            })
            .AddAppLogging();
    }
}
