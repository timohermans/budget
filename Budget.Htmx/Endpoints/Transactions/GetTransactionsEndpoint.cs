using Budget.Core.DataAccess;
using Budget.Core.UseCases.Transactions.Overview;
using Budget.Htmx.Components.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Budget.Htmx.Endpoints.Transactions;

public class GetTransactionsEndpoint : IEndpoint
{
    public void Configure(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/transactions", Handle)
            .WithName("transactions")
            .RequireAuthorization();
    }

    public async static Task<IResult> Handle([AsParameters] Request request, IDbContextFactory<BudgetContext> dbFactory)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var transactions = db.Transactions
            .Where(t => t.DateTransaction.Year == request.Date.Year &&
                        t.DateTransaction.Month == request.Date.Month &&
                        t.Iban == request.Iban);

        transactions = request.Direction == "asc"
            ? transactions.OrderBy(t => EF.Property<object>(t, request.OrderBy))
            : transactions.OrderByDescending(t => EF.Property<object>(t, request.OrderBy));

        return new RazorComponentResult<TransactionsSection>(new
        {
            Transactions = await transactions
                .Select(t => new OverviewTransaction(t))
                .ToListAsync(),
            OrderBy = request.OrderBy,
            Direction = request.Direction
        });
    }

    public record Request(DateTime Date, string Iban, string OrderBy, string Direction);
}