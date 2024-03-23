using Budget.Core.DataAccess;
using Budget.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Budget.Core.UseCases.Transactions.MarkAsCashback;

public class MarkAsCashbackUseCase(BudgetContext db, ILogger<MarkAsCashbackUseCase> logger)
{
    public async Task<Result> HandleAsync(MarkAsCashbackRequest request)
    {
        var transaction = await db.Transactions.SingleOrDefaultAsync(t => t.Id == request.Id);

        if (transaction == null)
        {
            return new ErrorResult("Transaction not found");
        }

        if (request.Date.HasValue)
        {
            transaction.CashbackForDate = DateOnly.FromDateTime(request.Date.Value);
        }
        else
        {
            transaction.CashbackForDate = null;
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "{Mark} {Transaction} with(out) cashback date {request.Date}",
            request.Date.HasValue ? "Marked" : "Unmarked", transaction.Id, request.Date);

        return new SuccessResult();
    }
}