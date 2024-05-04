using Budget.Core.DataAccess;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Budget.Htmx.Endpoints.Transactions;

public class PatchTransactionCashbackDateEndpoint : IEndpoint
{
    public const string RouteName = "transaction_cashback_patch";
    public const string CashbackPatchedTrigger = "transaction_cashback_patched";

    public void Configure(IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/transaction/{id:int}/cashback", Handle)
            .WithName(RouteName)
            .RequireAuthorization();
    }

    public async Task<IResult> Handle(int id, [FromForm] ViewModel request, IDbContextFactory<BudgetContext> dbFactory,
        HttpContext httpContext, HttpResponse response, LinkGenerator linker,
        ILogger<PatchTransactionCashbackDateEndpoint> logger)
    {
        logger.LogInformation("Patching transaction cashback date {Id} to {CashbackDate}", id,
            request.CashbackDate);
        await using var db = await dbFactory.CreateDbContextAsync();
        var transaction = await db.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return Results.NotFound();
        }

        if (DateTime.TryParse(request.CashbackDate, out DateTime cashbackForDate))
        {
            transaction.CashbackForDate = DateOnly.FromDateTime(cashbackForDate);
        }
        else
        {
            transaction.CashbackForDate = null;
        }

        await db.SaveChangesAsync();


        httpContext.Response.Headers.Append("Location",
            linker.GetPathByName(GetTransactionEndpoint.RouteName, new { Id = id }));
        response.Htmx(a => a.WithTrigger(CashbackPatchedTrigger, timing: HtmxTriggerTiming.AfterSwap));
        return await new GetTransactionEndpoint().Handle(new GetTransactionEndpoint.ViewModel(id), dbFactory);
    }

    public record ViewModel(string? CashbackDate);
}