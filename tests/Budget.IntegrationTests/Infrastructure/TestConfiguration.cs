using Microsoft.Extensions.Configuration;

namespace Budget.BlazorTests.Infrastructure;

internal class TestConfiguration
{
    private readonly IConfiguration config;

    public string Url => config.GetValue<string>("Url") ?? throw new NotImplementedException("Url missing");
    public string ApiBaseUrl => config.GetValue<string>("Api:BaseUrl") ?? throw new NotImplementedException("ApiBaseUrl missing");
    public string Username => config.GetValue<string>("User:Username") ?? throw new NotImplementedException("user missing");
    public string Password => config.GetValue<string>("User:Password") ?? throw new NotImplementedException("password missing");
    public string OtpSecret => config.GetValue<string>("User:OtpSecret") ?? throw new NotImplementedException("password missing");

    public TestConfiguration()
    {
        config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true)
            .AddUserSecrets<BlazorTest>()
            .AddEnvironmentVariables()
            .Build();
    }
}
