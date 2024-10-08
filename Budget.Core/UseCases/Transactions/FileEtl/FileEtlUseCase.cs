using Budget.Core.DataAccess;
using Budget.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Budget.Core.UseCases.Transactions.FileEtl;

public class FileEtlUseCase(IDbContextFactory<BudgetContext> dbFactory, ILogger<FileEtlUseCase> logger)
{
    public async Task<FileEtlResponse> HandleAsync(Stream stream)
    {
        var dataAccess = await dbFactory.CreateDbContextAsync();
        logger.LogInformation("Handling Transaction file upload");

        using var reader = new StreamReader(stream);

        await reader.ReadLineAsync(); // skip header

        var ibanIndex = 0;
        var currency = 1;
        var followNumberIndex = 3;
        var date = 4;
        var amount = 6;
        var balanceAfter = 7;
        var ibanOtherParty = 8;
        var nameOtherParty = 9;
        var authorizationCode = 16;
        int[] descriptionIndices = [19, 20, 21];

        List<Transaction> transactions = new();

        while (await reader.ReadLineAsync() is { } line)
        {
            var values = SplitLine(line).ToList();

            if (values.Count == 0)
            {
                logger.LogWarning("Skipping line somehow. Line is: {Line}", line);
                continue;
            }

            var followNumber = values.ElementAt(followNumberIndex);
            var dateTransaction = ParseCellToDate(values.ElementAt(date), date);
            var transaction = new Transaction
            {
                Iban = values.ElementAt(ibanIndex),
                Currency = values.ElementAt(currency),
                FollowNumber = ParseCellToInt(followNumber, followNumberIndex),
                DateTransaction = dateTransaction,
                Amount = ParseCellToDouble(values.ElementAt(amount), amount),
                BalanceAfterTransaction = ParseCellToDouble(values.ElementAt(balanceAfter), balanceAfter),
                IbanOtherParty = values.ElementAt(ibanOtherParty),
                NameOtherParty = values.ElementAt(nameOtherParty),
                AuthorizationCode = values.ElementAt(authorizationCode),
                Description = string.Join(" ",
                    descriptionIndices.Select(idx => values.ElementAt(idx))
                        .Where(v => !string.IsNullOrWhiteSpace(v)))
            };

            transactions.Add(transaction);
        }

        var minDate = transactions.Min(t => t.DateTransaction);
        var maxDate = transactions.Max(t => t.DateTransaction);

        await dataAccess.Transactions
            .Where(t => t.DateTransaction >= minDate && t.DateTransaction <= maxDate)
            .Select(t => new { t.FollowNumber, t.Iban, t.Id })
            .ForEachAsync(t =>
            {
                var transaction =
                    transactions.FirstOrDefault(t2 => t2.FollowNumber == t.FollowNumber && t2.Iban == t.Iban);

                if (transaction == null)
                {
                    logger.LogDebug("Transaction is new: {Transaction}", t);
                    return;
                }

                logger.LogDebug("Transaction exists already: {Transaction}", t);

                transaction.Id = t.Id;
                dataAccess.Entry(transaction).State = EntityState.Modified;
            });

        string? errorMessage = null;
        try
        {
            dataAccess.UpdateRange(transactions);
            await dataAccess.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            errorMessage = "Something went wrong bulk updating. See logs.";
            logger.LogError(ex, "Failed bulk updating transactions");
        }

        return new FileEtlResponse(transactions.Count, minDate.ToDateTime(new TimeOnly(0, 0, 0)),
            maxDate.ToDateTime(new TimeOnly(0, 0, 0)), errorMessage);
    }

    private IEnumerable<string> SplitLine(string line)
    {
        var values = new List<string>();

        for (int i = 0; i < line.Length - 1; i++)
        {
            if (line[i] == '"')
            {
                var startIndex = i + 1;
                var nextQuoteIndex = line.IndexOf('"', startIndex);
                var value = line.Substring(startIndex, nextQuoteIndex - i - 1);
                values.Add(value.Trim());
                i = nextQuoteIndex + 1;
            }
            else if (line[i] == ',')
            {
                values.Add("");
            }
            else
            {
                throw new ArgumentException("Unable to parse line at index " + i + " with value " + line[i] +
                                            " in line " + line);
            }
        }

        return values;
    }

    private int ParseCellToInt(string value, int columnIndex)
    {
        if (!int.TryParse(value, out int number))
        {
            logger.LogWarning("Unable to parse number {value} at column {columnIndex} to int", value, columnIndex);
        }

        return number;
    }

    private decimal ParseCellToDouble(string value, int columnIndex)
    {
        string decimalInput = value.Replace(",", ".");
        if (!decimal.TryParse(decimalInput, CultureInfo.InvariantCulture, out decimal number))
        {
            logger.LogWarning("Unable to parse number {value} at column {columnIndex} to int", value, columnIndex);
        }

        return number;
    }

    private DateOnly ParseCellToDate(string value, int columnIndex)
    {
        if (!DateOnly.TryParse(value, out var date))
        {
            logger.LogWarning("Unable to parse {value} at {columnIndex} to date", value, columnIndex);
        }

        return date;
    }
}