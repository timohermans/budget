using System.Security.Claims;
using Budget.Ui.Core.UseCases.Transactions.Overview;
using Budget.Ui.Server;
using Budget.Ui.States;
using Microsoft.AspNetCore.Components;

namespace Budget.Ui.Components.Pages;

public partial class Home : IDisposable
{
    [Parameter, SupplyParameterFromQuery] public int? Year { get; set; }
    [Parameter, SupplyParameterFromQuery] public int? Month { get; set; }
    [Parameter, SupplyParameterFromQuery] public string? Iban { get; set; }

    [Parameter] public OverviewResponse? Model { get; set; }
    [Parameter] public ClaimsPrincipal? User { get; set; }
    [Inject] public TimeProvider TimeProvider { get; set; } = null!;
    [Inject] public OverviewUseCase UseCase { get; set; } = null!;
    [Inject] public PersistentComponentState ApplicationState { get; set; } = null!;
    [Inject] public TransactionFilterState FilterState { get; set; } = null!;
    [Inject] public ILogger<Home> Logger { get; set; } = null!;

    private DateTime? _date;
    private PersistingComponentStateSubscription _persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Initializing Home component with Year={Year}, Month={Month}", Year, Month);
        FilterState.OnChange += FilterChanged;

        var now = TimeProvider.GetUtcNow().DateTime;
        var year = Year ?? now.Year;
        var month = Month ?? now.Month;
        _date = new DateTime(year, month, 1);

        _persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);

        if (ApplicationState.TryTakeFromJson<OverviewResponse>($"home.{_date}", out var restored))
        {
            Logger.LogInformation("Restored Home component state from ApplicationState for date {Date}", _date);
            Model = restored;
        }
        else
        {
            Logger.LogInformation("No restored state found for Home component for date {Date}, fetching data", _date);
            await GetDataAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        Logger.LogInformation("OnParametersSetAsync called with Year={Year}, Month={Month}", Year, Month);
        var now = TimeProvider.GetUtcNow().DateTime;
        var year = Year ?? now.Year;
        var month = Month ?? now.Month;
        var newDate =  new DateTime(year, month, 1);

        if (_date != newDate)
        {
            Logger.LogInformation("Date changed from {OldDate} to {NewDate}, fetching new data", _date, newDate);
            await GetDataAsync();
            _date = newDate;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async void FilterChanged()
    {
        Logger.LogDebug("Filter changed in home: {Filter}", FilterState.Dump());
        await GetDataAsync();
        await InvokeAsync(StateHasChanged);
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