using Budget.App.Apis.LoginLogout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Web;
using Microsoft.Identity.Client;

namespace Budget.App.Components.Layout;

public partial class MainLayout
{
    [Inject] ILogger<MainLayout> Logger { get; set; } = default!;

    [Inject] IConfiguration Config { get; set; } = default!;
    [Inject] IAuthorizationHeaderProvider AuthHeaderProvider { get; set; } = default!;
    [Inject] AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] NavigationManager Navigator { get; set; } = default!;

    [Inject] MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (!authState.User.Identity?.IsAuthenticated ?? false)
        {
            Logger.LogInformation("oopsie, relogin");
            var returnUrl = HttpUtility.UrlEncode(Navigator.Uri);
            Navigator.NavigateTo($"{LoginLogoutApi.LoginEndpoint}?returnUrl={returnUrl}", forceLoad: true);
        }
        
        await TryRefreshAuthToken();
    }

    private async Task TryRefreshAuthToken()
    {
        try
        {
            var scopes = Config.GetSection("Api:Scopes").Get<string[]>();
            await AuthHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes ?? []);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to retrieve api token");
            ConsentHandler.HandleException(ex);
        }
    }

    private void HandleUncaughtError(Exception exception)
    {
        Logger.LogError(exception, "Uncaught error");
        if (exception is MsalUiRequiredException or MicrosoftIdentityWebChallengeUserException)
        {
            ConsentHandler.HandleException(exception);
        }
    }
}