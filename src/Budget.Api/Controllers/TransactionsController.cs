using Budget.Api.Models;
using Budget.Application.UseCases.TransactionsFileJobStart;
using Budget.Application.UseCases.UpdateTransactionCashbackDate;
using Budget.Domain.Contracts;
using Budget.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class TransactionsController(ITransactionsFileJobStartUseCase useCase,
    IUpdateTransactionCashbackDateUseCase updateCashbackDateUseCase,
    ITransactionRepository transactionRepository) : ControllerBase
{
    [HttpPost("upload", Name = "PostTransactionsFile")]
    [ProducesResponseType(typeof(TransactionsFileJobStartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        var command = new TransactionsFileJobStartCommand
        {
            File = new TransactionsFileJobStartUseCase.FileModel
            {
                Size = file.Length,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = GetFileBytesFrom(file)
            }
        };

        var result = await useCase.HandleAsync(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet(Name = "GetTransactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string? iban)
    {
        var transactions = await transactionRepository.GetTransactionsByDateRangeAsync(startDate, endDate, iban);

        var response = transactions.Select(transaction => new TransactionResponseModel
        {
            Id = transaction.Id,
            FollowNumber = transaction.FollowNumber,
            Iban = transaction.Iban,
            Amount = transaction.Amount,
            DateTransaction = transaction.DateTransaction,
            NameOtherParty = transaction.NameOtherParty,
            IbanOtherParty = transaction.IbanOtherParty,
            AuthorizationCode = transaction.AuthorizationCode,
            Description = transaction.Description,
            CashbackForDate = transaction.CashbackForDate,
            Code = transaction.Code
        }).ToList();

        return Ok(response);
    }

    [HttpGet("ibans", Name="GetIbansDistinct")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDistinctIbans()
    {
        var ibans = await transactionRepository.GetAllDistinctIbansAsync();

        return Ok(ibans);
    }

    [HttpGet("cashflow-per-iban", Name="GetIbanCashflow")]
    [ProducesResponseType<CashflowDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCashflowPerIban([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string? iban)
    {
        var cashFlowPerIban = await transactionRepository.GetCashFlowPerIbanAsync(startDate, endDate, iban);

        return Ok(cashFlowPerIban);
    }

    [HttpPatch("{id}/cashback-date", Name = "UpdateCashbackForDate")]
    [ProducesResponseType(typeof(UpdateTransactionCashbackDateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCashbackForDate(int id, [FromBody] TransactionPatchCashbackDateCommandModel command)
    {
        var useCaseCommand = new UpdateTransactionCashbackDateCommand
        {
            TransactionId = id,
            CashbackForDate = command.CashbackForDate
        };

        var result = await updateCashbackDateUseCase.HandleAsync(useCaseCommand);

        if (result.IsFailure)
        {
            return result.Error.Contains("not found")
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    private byte[] GetFileBytesFrom(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}