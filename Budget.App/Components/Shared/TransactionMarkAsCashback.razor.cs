using Budget.App.Services;
using Microsoft.AspNetCore.Components;

namespace Budget.App.Components.Shared;

public partial class TransactionMarkAsCashback
{
    [Inject] private ApiClientProvider ApiProvider { get; set; } = default!;
    [Parameter] [EditorRequired] public OverviewTransaction Transaction { get; set; } = default!;
    [Parameter] public EventCallback<bool> OnDone { get; set; }

    private DateTime? _date;
    private bool _isLoading;

    protected override void OnParametersSet()
    {
        _date = Transaction.Date;
    }

    private async Task MarkAsCashbackAsync()
    {
        var api = await ApiProvider.ProvideAsync();
        if (api is null)
        {
            return;
        }

        _isLoading = true;
        await api.Transaction_MarkAsCashbackAsync(new MarkAsCashbackRequest { Id = Transaction.Id, Date = _date });
        await OnDone.InvokeAsync(true);
        _isLoading = false;
    }
}