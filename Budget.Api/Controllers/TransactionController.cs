using Budget.Core.Infrastructure;
using Budget.Core.UseCases.Transactions.FileEtl;
using Budget.Core.UseCases.Transactions.MarkAsCashback;
using Budget.Core.UseCases.Transactions.Overview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[Route("[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<OverviewResponse>(200)]
    public async Task<IActionResult> GetOverview([FromQuery] OverviewRequest request, [FromServices] OverviewUseCase useCase)
    {
        // TODO: Change overview to 'get', which can get all transaction by a start/end date. Let the client handle aggregation
        // TODO: Try implement pagination properly
        var response = await useCase.HandleAsync(request);
        return Ok(response);
    }
    
    [HttpPost("/upload")]
    public async Task<IActionResult> UploadTransactions(IFormFile file, [FromServices] FileEtlUseCase useCase)
    {
        var response = await useCase.HandleAsync(file.OpenReadStream());
        return Ok(response);
    }
    
    [HttpPatch("/mark-as-cashback")]
    public async Task<IActionResult> MarkAsCashback([FromBody] MarkAsCashbackRequest request, [FromServices] MarkAsCashbackUseCase useCase)
    {
        // TODO: Add ID to url, look up how patch requests should be done, and create request 
        var response = await useCase.HandleAsync(request);
        return response switch
        {
            SuccessResult => Ok(),
            ErrorResult error => BadRequest(error.Message),
            _ => BadRequest("Unknown error")
        };
    }
    
    
}
