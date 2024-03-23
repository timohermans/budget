using Budget.App.Apis.LoginLogout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Web;

namespace Budget.App.Services;

public class ApiClientProvider
{
    private readonly NavigationManager _navigator;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IAuthorizationHeaderProvider _authHeaderProvider;
    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ApiClientProvider> _logger;

    public ApiClientProvider(NavigationManager navigator, AuthenticationStateProvider authenticationStateProvider,
        IAuthorizationHeaderProvider authHeaderProvider,
        MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler, IHttpClientFactory clientFactory,
        IConfiguration config, ILogger<ApiClientProvider> logger)
    {
        _navigator = navigator;
        _authenticationStateProvider = authenticationStateProvider;
        _authHeaderProvider = authHeaderProvider;
        _consentHandler = consentHandler;
        _clientFactory = clientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<Client?> ProvideAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;

        var client = _clientFactory.CreateClient();
        var scopes = _config.GetSection("Api:Scopes").Get<string[]>();
        var url = _config.GetValue<string>("Api:BaseUrl") ?? "";
        client.BaseAddress = new Uri(url);

        if (!user.Identity?.IsAuthenticated ?? false)
        {
            var returnUrl = HttpUtility.UrlEncode(_navigator.Uri);
            _navigator.NavigateTo($"{LoginLogoutApi.LoginEndpoint}?returnUrl={returnUrl}", forceLoad: true);
            return null;
        }

        try
        {
            var authHeader = await _authHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes ?? []);
            client.DefaultRequestHeaders.Add("Authorization", authHeader);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _consentHandler.HandleException(ex);
        }

        var api = new Client(url, client);
        return api;
    }
}