using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Azure.Data.Tables;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Budget.IntegrationTests.Config;

// [Collection("web")]
public class TestFixture
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IContainer _container;

    public TestFixture()
    {
        _factory = new WebApplicationFactory<Program>();
        _container = new ContainerBuilder()
          .WithImage("mcr.microsoft.com/azure-storage/azurite")
          .WithPortBinding(10002, true)
          .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Azurite Table service is successfully listening at http://0.0.0.0:10002"))
          .Build();
    }

    /// <summary>
    /// inspired by https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedAppClientAsync(ITestOutputHelper? outputHelper = null)
    {
        if (_container.State != TestcontainersStates.Running)
        {
            outputHelper?.WriteLine("Starting container");
            await _container.StartAsync().ConfigureAwait(false);
        }

        if (_factory == null) throw new ArgumentNullException();
        var clientBuilder = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddJsonFile("appsettings.integration.json");
                });

                builder.ConfigureTestServices(services =>
                {
                    if (outputHelper != null)
                    {
                        services.AddLogging(logBuilder =>
                            logBuilder.ClearProviders().AddXunit(outputHelper));
                    }

                    services.AddAuthentication(defaultScheme: "TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "TestScheme", options => { });

                    services.RemoveAll<TableClient>();
                    services.AddScoped(_ =>
                    {
                        var service = new TableServiceClient($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://{_container.Hostname}:{_container.GetMappedPublicPort(10002)}/devstoreaccount1");
                        return service.GetTableClient("Transactions");
                    });
                });
            });

        using (var scope = clientBuilder.Services.CreateScope())
        {
            var tableClient = scope.ServiceProvider.GetRequiredService<TableClient>();
            await tableClient.DeleteAsync();
            await tableClient.CreateIfNotExistsAsync();
        };

        var client = clientBuilder.CreateClient(new WebApplicationFactoryClientOptions
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
public class IntegrationCollectionDefinition : ICollectionFixture<TestFixture>
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