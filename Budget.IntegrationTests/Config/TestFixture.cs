using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Azure.Data.Tables;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        var container = new ContainerBuilder()
          // Set the image for the container to "testcontainers/helloworld:1.1.0".
          .WithImage("mcr.microsoft.com/azure-storage/azurite")
          // Bind port 8080 of the container to a random port on the host.
          .WithPortBinding(10002, true)
          // Wait until the HTTP endpoint of the container is available.
          .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Azurite Table service is successfully listening at http://0.0.0.0:10002"))
          // Build the container configuration.
          .Build();

        testOutputHelper.WriteLine("Starting container");
        await container.StartAsync().ConfigureAwait(false);
        testOutputHelper.WriteLine("Container started");


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

                    services.RemoveAll<TableClient>();
                    services.AddScoped(_ =>
                    {
                        var service = new TableServiceClient($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://{container.Hostname}:{container.GetMappedPublicPort(10002)}/devstoreaccount1");
                        var client = service.GetTableClient("Transactions");
                        testOutputHelper.WriteLine($"Creating transaction table");
                        client.CreateIfNotExists();
                        return client;
                    });

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