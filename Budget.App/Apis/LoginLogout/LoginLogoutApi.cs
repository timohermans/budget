
namespace Budget.App.Apis.LoginLogout;

public static class LoginLogoutApi
{
    public const string LoginEndpoint = "/account/login";
    public const string TwoFactoryEndpoint = "/account/two-factor-login";
    public const string LogoutEndpoint = "/account/logout";
   
    public static RouteGroupBuilder MapLoginLogoutApis(this RouteGroupBuilder group)
    {
        group.MapGet(GetMapPartOf(LoginEndpoint), LoginController.GetLogin);
        group.MapPost(GetMapPartOf(LoginEndpoint), LoginController.PostLogin);
        group.MapGet(GetMapPartOf(TwoFactoryEndpoint), TwoFactorController.Get);
        group.MapPost(GetMapPartOf(TwoFactoryEndpoint), TwoFactorController.Post);

        return group;
    }

    private static string GetMapPartOf(string endpoint)
    {
        var lastIndex = endpoint.LastIndexOf("/", StringComparison.Ordinal);
        return endpoint.Substring(lastIndex, endpoint.Length - lastIndex);
    }
    

}