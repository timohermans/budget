using Budget.App.Services;

namespace Budget.App.Config;

public static class ServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton(_ => TimeProvider.System);
        services.AddHttpClient();
        services.AddScoped<ApiClientProvider>();
        //services.AddHttpClient<Client>(async (services, client) =>
        //{
        //    var config = services.GetRequiredService<IConfiguration>();
        //    var scopes = config.GetSection("Api:Scopes").Get<string[]>();
        //    var authHeaderProvider = services.GetService<IAuthorizationHeaderProvider>();
        //    var baseUrl = config.GetValue<string>("Api:BaseUrl") ?? "";

        //    client.BaseAddress = new Uri(baseUrl);

        //    if (authHeaderProvider is not null)
        //    {
        //        var authHeader = await authHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes ?? []);
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authHeader);
        //    }
        //});

        return services;
    }
}