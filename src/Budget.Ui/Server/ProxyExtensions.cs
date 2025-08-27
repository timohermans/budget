using Microsoft.AspNetCore.HttpOverrides;

namespace Budget.Ui.Server;

public static class ProxyExtensions
{
    public static IServiceCollection AddProxyConfig(this IServiceCollection services, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        return services;
    }
}