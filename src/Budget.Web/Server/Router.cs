using Budget.Web.Endpoints;

namespace Budget.Web.Server;

public static class Router
{
    public static void UseBudgetRoutes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", GetTransactionsOverviewEndpoint.Handle);
    }
}