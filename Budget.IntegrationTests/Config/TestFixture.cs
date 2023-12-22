using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Budget.IntegrationTests.Config;

public class TestFixture(WebApplicationFactory<Program> _factory, ITestOutputHelper testOutputHelper) : IClassFixture<WebApplicationFactory<Program>>
{
    /// <summary>
    /// inspired by https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedAppClientAsync()
    {
        testOutputHelper.WriteLine("Creating authenticated client");
        if (_factory == null) throw new ArgumentNullException();
        var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddJsonFile("appsettings.integration.json");
                });

                builder.ConfigureTestServices(services =>
                {
                    services.AddLogging(logBuilder =>
                        logBuilder.ClearProviders().AddXunit(testOutputHelper));

                    services.AddAuthentication(defaultScheme: "TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "TestScheme", options => { });

                    // TODO: Make sure that test Azurite is used
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false, // this makes sure you will not get a 200 at the /login page automatically
            });

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "TestScheme");

        var response = await client.GetAsync("/transactions");

        return client ?? throw new NullReferenceException("Something went wrong creating the client");
    }
}

[CollectionDefinition("integration")]
public class IntegrationCollectionDefinition : ICollectionFixture<WebApplicationFactory<Program>>
{
}

public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Timo") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}