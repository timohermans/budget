using Budget.App.Server;
using Budget.App.States;
using Budget.Core.UseCases.Transactions.Overview;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace Budget.App.Components.Pages;

public partial class Home : IDisposable
{
    [Parameter, SupplyParameterFromQuery] public int? Year { get; set; }
    [Parameter, SupplyParameterFromQuery] public int? Month { get; set; }
    [Parameter, SupplyParameterFromQuery] public string? Iban { get; set; }

    [Parameter] public OverviewResponse? Model { get; set; }
    [Parameter] public ClaimsPrincipal? User { get; set; }
    [Inject] public TimeProvider TimeProvider { get; set; } = default!;
    [Inject] public OverviewUseCase UseCase { get; set; } = default!;
    [Inject] public PersistentComponentState ApplicationState { get; set; } = default!;
    [Inject] public TransactionFilterState FilterState { get; set; } = default!;
    [Inject] public ILogger<Home> Logger { get; set; } = default!;

    private DateTime? _date;
    private PersistingComponentStateSubscription _persistingSubscription;

    protected override void OnInitialized()
    {
        FilterState.OnChange += FilterChanged;
    }

    public async void FilterChanged()
    {
        Logger.LogDebug("Filter changed in home: {Filter}", FilterState.Dump());
        await GetDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnParametersSetAsync()
    {
        _persistingSubscription =
            ApplicationState.RegisterOnPersisting(PersistData);

        if (!ApplicationState.TryTakeFromJson<OverviewResponse>($"home.{_date}", out var restored))
        {
            await GetDataAsync();
        }
        else
        {
            Model = restored;
        }
    }

    private async Task GetDataAsync()
    {
        var now = TimeProvider.GetUtcNow().DateTime;
        var year = Year ?? now.Year;
        var month = Month ?? now.Month;
        _date = new DateTime(year, month, 1);

        Model = await UseCase.HandleAsync(new OverviewRequest
        {
            Iban = Iban,
            Year = year,
            Month = month,
            OrderBy = FilterState.OrderedBy,
            Direction = FilterState.Direction
        });
    }

    private Task PersistData()
    {
        ApplicationState.PersistAsJson($"home.{_date}", Model);
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
        FilterState.OnChange -= FilterChanged;
        _persistingSubscription.Dispose();
    }
}
