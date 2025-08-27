using Budget.ApiClient;
using Budget.Ui.Core.Models;

namespace Budget.Ui.Core.UseCases.Transactions.MarkAsCashback;

public class MarkAsCashbackUseCase(IBudgetClient httpClient, ILogger<MarkAsCashbackUseCase> logger)
{
    public async Task<Result> HandleAsync(MarkAsCashbackRequest request)
    {
        
        await httpClient.UpdateCashbackForDateAsync(request.Id,
            new TransactionPatchCashbackDateCommandModel
            {
                CashbackForDate = request.Date
            });

        logger.LogInformation(
            "{Mark} {Transaction} with(out) cashback date {Date}",
            request.Date.HasValue ? "Marked" : "Unmarked", request.Id, request.Date);

        return new SuccessResult();
    }
}