using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AngleSharp;
using AngleSharp.Dom;
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

public class TestFixture : IDisposable
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
    /// Opens html string content into a DOM document like object that your can QuerySelector on
    /// </summary>
    /// <param name="htmlContent">response from `await client.GetAsync("url");`</param>
    public async Task<IDocument> OpenHtmlOf(HttpContent htmlContent)
    {
        var browser = BrowsingContext.New(Configuration.Default);
        var contentStream = await htmlContent.ReadAsStreamAsync();
        var document = await browser.OpenAsync(req => req.Content(contentStream));
        return document ?? throw new NullReferenceException("Something went wrong opening the html");
    }
    
    /// <summary>
    /// Opens connection to transaction azure table and clears it. Use this when you only want to use table, not html
    /// </summary>
    /// <returns>The table client to the transactions table</returns>
    public async Task<TableClient> CreateTableClientAsync()
    {
        if (_container.State != TestcontainersStates.Running)
        {
            await _container.StartAsync().ConfigureAwait(false);
        }

        var service = new TableServiceClient($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://{_container.Hostname}:{_container.GetMappedPublicPort(10002)}/devstoreaccount1");
        var client = service.GetTableClient("Transactions");
        await client.DeleteAsync();
        await client.CreateIfNotExistsAsync();
        return client;
    }

    /// <summary>
    /// inspired by https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedAppClientAsync(ITestOutputHelper? outputHelper = null)
    {
        await EnsureDatabaseIsRunning(outputHelper);

        if (_factory == null) throw new ArgumentNullException();

        var clientBuilder = ConfigureClient(outputHelper);
        await EnsureTransactionTableIsClearedAsync(clientBuilder);
        var client = CreateClient(clientBuilder);

        return client ?? throw new NullReferenceException("Something went wrong creating the client");
    }

    private async Task EnsureDatabaseIsRunning(ITestOutputHelper? outputHelper)
    {
        if (_container.State != TestcontainersStates.Running)
        {
            outputHelper?.WriteLine("Starting container");
            await _container.StartAsync().ConfigureAwait(false);
        }
    }

    private WebApplicationFactory<Program> ConfigureClient(ITestOutputHelper? outputHelper)
    {
        return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, conf) =>
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
                            "TestScheme", _ => { });

                    services.RemoveAll<TableClient>();
                    services.AddScoped(_ =>
                    {
                        var service = new TableServiceClient($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://{_container.Hostname}:{_container.GetMappedPublicPort(10002)}/devstoreaccount1");
                        return service.GetTableClient("Transactions");
                    });
                });
            });
    }

    private async Task EnsureTransactionTableIsClearedAsync(WebApplicationFactory<Program> clientBuilder)
    {
        using var scope = clientBuilder.Services.CreateScope();
        var tableClient = scope.ServiceProvider.GetRequiredService<TableClient>();
        await tableClient.DeleteAsync();
        await tableClient.CreateIfNotExistsAsync();
    }

    private HttpClient CreateClient(WebApplicationFactory<Program> clientBuilder)
    {
        var client = clientBuilder.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false, // this makes sure you will not get a 200 at the /login page automatically
        });

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(scheme: "TestScheme");

        return client;
    }

    public void Dispose()
    {
        _container.DisposeAsync();
    }
}


[CollectionDefinition("integration")]
public class IntegrationCollectionDefinition : ICollectionFixture<TestFixture>;

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