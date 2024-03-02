namespace Budget.BlazorTests.Infrastructure;

public class BlazorTest : PageTest
{
    private const string StateFilePath = "state.json";
    private readonly AppHelper appHelper;
    private readonly TestConfiguration config;

    public DatabaseHelper DatabaseHelper { get; } = new();

    public BlazorTest()
    {
        appHelper = new AppHelper(DatabaseHelper);
        config = new TestConfiguration();
    }

    protected Uri RootUri { get; private set; } = default!;

    [OneTimeSetUp]
    public async Task SetUpWebApplication()
    {
        await DatabaseHelper.LaunchAsync();
        RootUri = await appHelper.LaunchAsync(config.Url);
    }

    [SetUp]
    public async Task BeforeEach()
    {
        await DatabaseHelper.ResetDataAsync();
    }

    [OneTimeTearDown]
    public async Task TearDownWebApplication()
    {
        await DatabaseHelper.StopAsync();
        await appHelper.StopAsync();
    }

    public async Task GotoAsync(string page)
    {
        await Page.GotoAsync($"{RootUri.AbsoluteUri}{page}", new() { WaitUntil = WaitUntilState.NetworkIdle });
        if (await Page.GuardAgainstUnauthenticated(Browser, Context, RootUri, config.Username, config.Password))
        {
            await Page.GotoAsync($"{RootUri.AbsoluteUri}{page}", new() { WaitUntil = WaitUntilState.NetworkIdle });
        }
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        if (!File.Exists(StateFilePath))
        {
            File.WriteAllText(StateFilePath, "{}");
        }
        return new BrowserNewContextOptions
        {
            StorageStatePath = StateFilePath
        };
    }
}
