using Hertmans.Shared.Auth;

namespace Budget.Ui.Server.Options;

public class BudgetApiOptions : IApiClientOptions
{
    public string BaseUrl { get; set; } = null!;
    public string Authority { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string[] Scopes { get; set; } = [];
}