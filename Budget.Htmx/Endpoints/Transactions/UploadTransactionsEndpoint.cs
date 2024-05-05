using Budget.Core.UseCases.Transactions.FileEtl;
using Budget.Htmx.Components.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Htmx.Endpoints.Transactions;

public class UploadTransactionsEndpoint : IEndpoint
{
    public static string RouteName => "transactions_upload";

    public void Configure(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/transactions/upload", Handle)
            .WithName(RouteName)
            .RequireAuthorization();
    }

    public async Task<IResult> Handle([FromForm] ViewModel request, FileEtlUseCase useCase, HttpContext httpContext,
        LinkGenerator linker, ILogger<UploadTransactionsEndpoint> logger)
    {
        var response = await useCase.HandleAsync(
            request.File.OpenReadStream()
        );

        httpContext.Response.Headers.Append("Location", linker.GetPathByName("budget_overview"));
        return Results.StatusCode(303);

        // TODO: if something goes wrong
        // return new RazorComponentResult<UploadTransactions>(new
        // {
        //     Model = response
        // });
    }

    public record ViewModel(IFormFile File);
}