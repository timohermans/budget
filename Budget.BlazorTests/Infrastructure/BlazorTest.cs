namespace Budget.BlazorTests.Infrastructure;

public class BlazorTest : PageTest
{
    private const string StateFilePath = "state.json";
    private readonly AppHelper _appHelper;
    private readonly TestConfiguration _config;

    protected DatabaseHelper DatabaseHelper { get; } = new();

    public BlazorTest()
    {
        _appHelper = new AppHelper(DatabaseHelper);
        _config = new TestConfiguration();
    }

    protected Uri RootUri { get; private set; } = default!;

    [OneTimeSetUp]
    public async Task SetUpWebApplication()
    {
        await DatabaseHelper.LaunchAsync();
        RootUri = _appHelper.Launch(_config.Url, _config.ApiBaseUrl);
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
        await _appHelper.StopAsync();
    }

    public async Task GotoAsync(string page)
    {
        await Page.GotoAsync($"{RootUri.AbsoluteUri}{page}", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.GuardAgainstUnauthenticated(Browser, Context, RootUri, _config.Username, _config.Password, _config.OtpSecret);
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
