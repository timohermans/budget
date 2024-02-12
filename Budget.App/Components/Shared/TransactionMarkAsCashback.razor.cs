using Budget.Core.DataAccess;
using Budget.Core.Models;
using Budget.Core.UseCases.Transactions.MarkAsCashback;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Budget.App.Components.Shared;

public partial class TransactionMarkAsCashback
{
    [Inject] private IDbContextFactory<BudgetContext> DbFactory { get; set; } = default!;
    [Inject] private ILogger<UseCase> UseCaseLogger { get; set; } = default!;
    [Parameter] [EditorRequired] public Transaction Transaction { get; set; } = default!;
    [Parameter] public EventCallback<Transaction?> OnDone { get; set; }

    private DateOnly? _date;
    private bool _isLoading;

    protected override void OnParametersSet()
    {
        _date = Transaction.DateTransaction;
    }
    
    private async Task MarkAsCashbackAsync()
    {
        _isLoading = true;
        await using var db = await DbFactory.CreateDbContextAsync();
        var useCase = new UseCase(db, UseCaseLogger);
        var transactionMarked = await useCase.Handle(new Request(Transaction.Id, _date));
        await OnDone.InvokeAsync(transactionMarked.Data);
        _isLoading = false;
    }
    
}