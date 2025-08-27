using Budget.App.States;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using Budget.App.Core.UseCases.Transactions.Overview;

namespace Budget.App.Components.Shared;

public partial class TransactionsSection : IDisposable
{
    [Parameter, EditorRequired] public IEnumerable<OverviewTransaction> Transactions { get; set; } = [];
    [Parameter, EditorRequired] public EventCallback OnTransactionUpdate { get; set; } = default!;
    [Inject] public TransactionFilterState FilterState { get; set; } = default!;
    [Inject] public ILogger<TransactionsSection> Logger { get; set; } = default!;

    private TransactionTableHeader.HeaderData? _headerData;

    private readonly CultureInfo _dutch = new("nl-NL");

    protected override void OnInitialized()
    {
        _headerData = new TransactionTableHeader.HeaderData(FilterState.OrderedBy, FilterState.Direction);
        FilterState.OnChange += OnFilterChange;
    }

    private void OnFilterChange()
    {
        _headerData = new TransactionTableHeader.HeaderData(FilterState.OrderedBy, FilterState.Direction);
        StateHasChanged();
    }

    public void Dispose()
    {
        FilterState.OnChange -= OnFilterChange;
    }
}
