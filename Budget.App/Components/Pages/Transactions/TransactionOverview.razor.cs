using Budget.App.Components.Shared;
using Budget.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.JSInterop;
using System.Globalization;
using System.Security.Claims;

namespace Budget.App.Components.Pages.Transactions;

public partial class TransactionOverview
{
    [Inject] private ILogger<TransactionOverview> Logger { get; set; } = null!;
    [Inject] private NavigationManager Navigator { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ApiClientProvider ApiProvider { get; set; } = default!;

    private Client? _api;
    private DateOnly? _date;
    private OverviewResponse? _data;
    private readonly CultureInfo _dutch = new("nl-NL");
    private ClaimsPrincipal? _user;
    private int? _transactionIdMarkingAsCashback = null;
    private bool _areSavingsMetersVisible;

    private readonly GridSort<OverviewTransaction> _fixedSort = GridSort<OverviewTransaction>
        .ByDescending(p => p.IsFixed)
        .ThenDescending(p => p.Date);

    [SupplyParameterFromQuery(Name = "year")]
    public int? Year { get; set; }

    [SupplyParameterFromQuery(Name = "month")]
    public int? Month { get; set; }

    [SupplyParameterFromQuery(Name = "iban")]
    public string? Iban { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        _user = state.User;
        _api = await ApiProvider.ProvideAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JsRuntime.InvokeVoidAsync("enablePopovers");
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_api is null) { return; }
        Logger.LogInformation("Loading transactions for {Year}-{Month} -> {Iban}", _date?.Year, _date?.Month, Iban);

        if (Year == null || Month == null)
        {
            _date = DateOnly.FromDateTime(Time.GetUtcNow().DateTime);
        }
        else
        {
            _date = new DateOnly(Year.Value, Month.Value, 1);
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        if (_api is null) { return; }
        if (_date is null)
        {
            Logger.LogWarning("Trying to load data without a date");
            return;
        }

        _data = await _api.Transaction_GetOverviewAsync(_date.Value.Year, _date.Value.Month, Iban);
    }

    private IQueryable<OverviewTransaction> GetTransactions()
    {
        return _data?.Transactions?
            .AsQueryable()
            .OrderByDescending(t => t.Date)
        ?? new List<OverviewTransaction>().AsQueryable();
    }

    private string GetPreviousDate()
    {
        var dateNext = _date?.AddMonths(-1);
        return $"/transactions?year={dateNext?.Year}&month={dateNext?.Month}&iban={Iban}";
    }

    private string GetNextDate()
    {
        var dateNext = _date?.AddMonths(1);
        return $"/transactions?year={dateNext?.Year}&month={dateNext?.Month}&iban={Iban}";
    }

    private string GetToday()
    {
        var dateNext = DateOnly.FromDateTime(Time.GetUtcNow().DateTime);
        return $"/transactions?year={dateNext.Year}&month={dateNext.Month}&iban={Iban}";
    }

    private void SwitchIban()
    {
        Navigator.NavigateTo($"/transactions?year={_date?.Year}&month={_date?.Month}&iban={Iban}");
    }

    private async Task HandleCashbackDone()
    {
        Logger.LogInformation("Marked transaction {TransactionId} as cashback", _transactionIdMarkingAsCashback);

        _transactionIdMarkingAsCashback = null;

        await LoadData();
        await JsRuntime.InvokeVoidAsync("enablePopovers");
    }

    private void HandleOptionSelected(TransactionRowOptions.Options option, int transactionId)
    {
        if (option == TransactionRowOptions.Options.MarkAsCashback)
        {
            _transactionIdMarkingAsCashback = transactionId;
        }
    }
}