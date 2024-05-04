namespace Budget.Htmx.Endpoints.LoginLogout;

public static class LoginLogoutApi
{
    public const string GroupName = "/account";
    public const string LoginEndpoint = "/account/login";
    public const string LogoutEndpoint = "/account/logout";

    public static RouteGroupBuilder MapLoginLogoutApis(this RouteGroupBuilder group)
    {
        group.MapGet(GetMapPartOf(LoginEndpoint), LoginController.GetLogin);
        group.MapGet(GetMapPartOf(LogoutEndpoint), LogoutController.Get);

        return group;
    }

    private static string GetMapPartOf(string endpoint)
    {
        var lastIndex = endpoint.LastIndexOf("/", StringComparison.Ordinal);
        return endpoint.Substring(lastIndex, endpoint.Length - lastIndex);
    }


}