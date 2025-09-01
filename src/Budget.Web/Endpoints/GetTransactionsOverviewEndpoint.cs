using Budget.Web.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Budget.Web.Endpoints;

public class GetTransactionsOverviewEndpoint
{
    public static IResult Handle()
    {
        return new RazorComponentResult<Transactions>(new { Args = new Transactions.TransactionsArgs("Timo") });
    }
}