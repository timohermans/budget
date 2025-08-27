using Budget.App.Core.UseCases.Transactions.MarkAsCashback;
using Budget.App.Core.UseCases.Transactions.Overview;
using Microsoft.AspNetCore.Components;

namespace Budget.App.Components.Shared;

public partial class TransactionCashbackForm
{
    [Inject] public MarkAsCashbackUseCase UseCase { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Parameter, EditorRequired] public OverviewTransaction Transaction { get; set; } = default!;
    [Parameter, EditorRequired] public EventCallback<bool> OnClose { get; set; } = default!;

    [SupplyParameterFromForm] private CashbackModel? Model { get; set; }
    private readonly bool _isSubmitting = false;

    protected override void OnParametersSet() => Model ??= new CashbackModel { Date = Transaction.Date };

    private async Task MarkAsCashbackAsync()
    {
        var result = await UseCase.HandleAsync(new MarkAsCashbackRequest(Transaction.Id, Model?.Date));
        await OnClose.InvokeAsync(result.IsSuccess);
    }

    private class CashbackModel
    {
        public DateTime? Date { get; set; }
    }

}
