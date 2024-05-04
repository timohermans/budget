using Budget.Core.UseCases.Transactions.Overview;
using Budget.Htmx.Components.Pages;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Budget.Htmx.Endpoints.Budget;

public class GetBudgetOverviewEndpoint : IEndpoint
{
    public void Configure(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/budget", Handle)
            .WithName("budget_overview")
            .RequireAuthorization();
    }

    public async Task<IResult> Handle([AsParameters] ViewModel request, OverviewUseCase useCase,
        HttpContext httpContext)
    {
        var year = request.Year ?? 2024;
        var month = request.Month ?? 1;
        var date = new DateTime(year, month, 1);
        var response = await useCase.HandleAsync(new OverviewRequest
        {
            Iban = request.Iban,
            Year = request.Year ?? 2024,
            Month = request?.Month ?? 1
        });

        return new RazorComponentResult<BudgetOverview>(new
        {
            Date = date,
            Model = response,
            User = httpContext.User
        });
    }

    public record ViewModel(int? Year, int? Month, string? Iban);
}