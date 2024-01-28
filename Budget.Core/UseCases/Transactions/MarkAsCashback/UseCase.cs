using Budget.Core.DataAccess;
using Budget.Core.Infrastructure;
using Budget.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Budget.Core.UseCases.Transactions.MarkAsCashback;

public class UseCase(BudgetContext db, ILogger<UseCase> logger)
{
    public async Task<Result<Transaction>> Handle(Request request)
    {
        var transaction = await db.Transactions.SingleOrDefaultAsync(t => t.Id == request.Id);

        if (transaction == null)
        {
            return new ErrorResult<Transaction>("Transaction not found");
        }

        if (request.Date.HasValue)
        {
            transaction.CashbackForDate = request.Date.Value;
        }
        else
        {
            transaction.CashbackForDate = null;
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "{Mark} {Transaction} with(out) cashback date {request.Date}",
            request.Date.HasValue ? "Marked" : "Unmarked", transaction.Id, request.Date);

        return new SuccessResult<Transaction>(transaction);
    }
}