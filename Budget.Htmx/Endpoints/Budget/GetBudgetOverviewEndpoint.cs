using Budget.Core.UseCases.Transactions.Overview;
using Budget.Htmx.Components.Pages;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Budget.Htmx.Endpoints.Budget;

public class GetBudgetOverviewEndpoint : IEndpoint
{
    public void Configure(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", Handle)
            .WithName("budget_overview")
            .RequireAuthorization();
    }

    public async Task<IResult> Handle([AsParameters] ViewModel request, OverviewUseCase useCase,
        HttpContext httpContext, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().DateTime;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;
        var date = new DateTime(year, month, 1);
        var response = await useCase.HandleAsync(new OverviewRequest
        {
            Iban = request.Iban,
            Year = year,
            Month = month
        });

        return new RazorComponentResult<BudgetOverview>(new
        {
            Date = date,
            Model = response,
            httpContext.User
        });
    }

    public record ViewModel(int? Year, int? Month, string? Iban);
}