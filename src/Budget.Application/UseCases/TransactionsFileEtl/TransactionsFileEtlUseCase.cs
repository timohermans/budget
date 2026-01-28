using Budget.Domain;
using Budget.Domain.Entities;
using Budget.Domain.Extensions;
using Budget.Domain.Repositories;
using CsvHelper;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Budget.Application.UseCases.TransactionsFileEtl;

public interface ITransactionsFileEtlUseCase
{
    Task<Result> HandleAsync(Stream stream, string user);
}

public class TransactionsFileEtlUseCase(ITransactionRepository repo, ILogger<TransactionsFileEtlUseCase> logger)
    : ITransactionsFileEtlUseCase
{
    public async Task<Result> HandleAsync(Stream stream, string user)
    {
        logger.LogInformation("Handling Transaction file upload");

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(new CultureInfo("nl-NL"))
        {
            HasHeaderRecord = true,
            Delimiter = ","
        });

        var records = csv.GetRecords<TransactionsFileCsvMap>();
        
#if DEBUG
        stream.Seek(0, SeekOrigin.Begin);
        using (var r = new StreamReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            string value = await r.ReadToEndAsync();
            logger.LogInformation("trans file contents: {CsvString}", value);
            stream.Seek(0, SeekOrigin.Begin);
        }
#endif

        List<Transaction> transactions = new();

        foreach (var r in records)
        {
            if (r.Iban == null || r.Currency == null || r.Amount == null || r.BalanceAfter == null ||
                r.IbanOtherParty == null || r.NameOtherParty == null || r.AuthorizationCode == null)
            {
                logger.LogWarning("Something is wrong/empty with this row: {Row}. Skipping...", r.Dump());
                continue;
            }

            transactions.Add(new Transaction
            {
                Iban = r.Iban,
                Currency = r.Currency,
                DateTransaction = r.Date,
                FollowNumber = r.FollowNumber,
                Amount = r.Amount.Value,
                AuthorizationCode = r.AuthorizationCode,
                BalanceAfterTransaction = r.BalanceAfter.Value,
                IbanOtherParty = r.IbanOtherParty,
                NameOtherParty = r.NameOtherParty,
                Description = (r.Description1 + r.Description2 + r.Description3).Trim(),
                Code = r.Code,
                BatchId = r.BatchId,
                Reference = r.Reference,
                User = user
            });
        }

        var minDate = transactions.Min(t => t.DateTransaction);
        var maxDate = transactions.Max(t => t.DateTransaction);

        var transactionIdsDb = (await repo.GetIdsBetweenAsync(minDate, maxDate)).ToList();
        List<Transaction> transactionsToAdd = [];

        foreach (var t in transactions)
        {
            var isInDb =
                transactionIdsDb.Any(t2 => t2.FollowNumber == t.FollowNumber && t2.Iban == t.Iban);

            if (isInDb)
            {
                logger.LogDebug("Transaction exists already: {Transaction}", t.Dump());
                continue;
            }

            transactionsToAdd.Add(t);
        }

        try
        {
            await repo.AddRangeAsync(transactionsToAdd);
            await repo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed bulk updating transactions");
            return Result.Failure("Something went wrong bulk updating. See logs.");
        }

        return Result.Success();
    }
}