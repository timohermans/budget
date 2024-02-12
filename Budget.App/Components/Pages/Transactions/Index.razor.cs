using System.Globalization;
using System.Security.Claims;
using Budget.App.Components.Shared;
using Budget.Core.DataAccess;
using Budget.Core.Models;
using Budget.Core.UseCases.Transactions.Overview;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Budget.App.Components.Pages.Transactions;

public partial class Index
{
    [Inject] private ILogger<Index> Logger { get; set; } = null!;
    [Inject] private IDbContextFactory<BudgetContext> DbContextFactory { get; set; } = null!;
    [Inject] private NavigationManager Navigator { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private DateOnly? _date;
    private Response? _data;
    private readonly CultureInfo _dutch = new CultureInfo("nl-NL");
    private ClaimsPrincipal? _user;
    private int? _transactionIdMarkingAsCashback = null;

    private readonly GridSort<Transaction> _fixedSort = GridSort<Transaction>
        .ByDescending(p => p.IsFixed)
        .ThenDescending(p => p.DateTransaction);

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
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JsRuntime.InvokeVoidAsync("enablePopovers");
    }

    protected override async Task OnParametersSetAsync()
    {
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
        await using var db = DbContextFactory.CreateDbContext();
        var useCase = new UseCase(db);

        if (_date == null)
        {
            Logger.LogWarning("Trying to load data without a date");
            return;
        }

        _data = await useCase.HandleAsync(new Request
        {
            Year = _date.Value.Year,
            Month = _date.Value.Month,
            Iban = Iban
        });
    }


    private IQueryable<Transaction> GetTransactions()
    {
        return _data?.TransactionsPerWeek?.SelectMany(kvp => kvp.Value)?
            .AsQueryable() ?? new List<Transaction>().AsQueryable();
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

    private async Task HandleCashbackDone(Transaction? transactionMarked)
    {
        _transactionIdMarkingAsCashback = null;

        Logger.LogInformation("Marked transaction {TransactionId} as cashback", transactionMarked?.Id);

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