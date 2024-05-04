using Budget.Core.DataAccess;
using Budget.Core.UseCases.Transactions.Overview;
using Budget.Htmx.Components.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Budget.Htmx.Endpoints.Transactions;

public class GetTransactionEndpoint : IEndpoint
{
    public static string RouteName = "transaction_get";
    public void Configure(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/transaction/{id:int}", Handle)
            .WithName(RouteName)
            .RequireAuthorization();
    }

    public async Task<IResult> Handle([AsParameters] ViewModel request, IDbContextFactory<BudgetContext> dbFactory)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var transaction = await db.Transactions.FindAsync(request.Id);
        
        if (transaction is null)
        {
            return Results.NotFound();
        }

        return new RazorComponentResult<TransactionRow>(new
        {
            Transaction = new OverviewTransaction(transaction)
        });
    }

    public record ViewModel(int Id);
}