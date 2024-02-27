using AngleSharp;
using AngleSharp.Dom;
using Budget.Core.DataAccess;
using Budget.IntegrationTests.Helpers;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace Budget.IntegrationTests.Config;

public class TestFixture : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory = new();

    private readonly IContainer _container = new PostgreSqlBuilder()
        .WithDatabase("budget")
        .WithUsername("budget")
        .WithPassword("budget")
        .Build();

    /// <summary>
    /// Opens html string content into a DOM document like object that your can QuerySelector on
    /// </summary>
    /// <param name="htmlContent">response from `await client.GetAsync("url");`</param>
    public async Task<IDocument> OpenHtmlOfAsync(HttpContent htmlContent)
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
    public async Task<BudgetContext> CreateTableClientAsync(bool ensureCleanDb = true)
    {
        var options = new DbContextOptionsBuilder<BudgetContext>()
            .UseNpgsql(GetDbConnectionString())
            .Options;

        var db = new BudgetContext(options);

        if (ensureCleanDb && await db.Database.EnsureCreatedAsync() == false)
        {
            await db.Database.ExecuteSqlRawAsync(@"DELETE FROM transactions;");
        }

        return new BudgetContext(options);
    }

    private string GetDbConnectionString()
    {
        return
            $"Server={_container.Hostname};Port={_container.GetMappedPublicPort(5432)};Database=budget;User Id=budget;Password=budget;";
    }

    /// <summary>
    /// inspired by https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0
    /// </summary>
    public Task<HttpClient> CreateAuthenticatedAppClientAsync(ITestOutputHelper? outputHelper = null,
        TimeProvider? timeProviderOverride = null, bool ensureCleanDb = true)
    {
        if (_factory == null)
        {
            throw new ArgumentNullException();
        }

        var clientBuilder = ConfigureClient(outputHelper, timeProviderOverride);
        var client = CreateClient(clientBuilder, ensureCleanDb);

        return Task.FromResult(client) ?? throw new NullReferenceException("Something went wrong creating the client");
    }

    private WebApplicationFactory<Program> ConfigureClient(ITestOutputHelper? outputHelper,
        TimeProvider? timeProviderOverride)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
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

                var descriptor =
                    services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BudgetContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton(timeProviderOverride ?? TimeProvider.System);
                services.AddDbContext<BudgetContext>(options =>
                    options.UseNpgsql(GetDbConnectionString()));
            });
        });
    }

    private HttpClient CreateClient(WebApplicationFactory<Program> clientBuilder, bool ensureCleanDb)
    {
        using var serviceScope = clientBuilder.Services.CreateScope();
        var db = (BudgetContext?)serviceScope.ServiceProvider.GetService(typeof(BudgetContext));

        if (db != null && db.Database.EnsureCreated() == false && ensureCleanDb)
        {
            db.Database.ExecuteSqlRaw("DELETE FROM transactions;");
        }

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

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }
}

[CollectionDefinition("integration")]
public class IntegrationCollectionDefinition : ICollectionFixture<TestFixture>;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
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