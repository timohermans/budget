using Budget.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Budget.Core.UseCases
{
    public class TransactionFileUploadUseCase(BudgetContext db, ILogger logger)
    {
        public record Response(int AmountInserted, DateOnly DateMin, DateOnly DateMax);

        public async Task<Response> HandleAsync(Stream stream)
        {
            logger.LogInformation("Handling Transaction file upload");

            List<Transaction> transactions = new List<Transaction>();

            using var reader = new StreamReader(stream);

            reader.ReadLine();

            var iban = 0;
            var currency = 1;
            var followNumber = 3;
            var date = 4;
            var amount = 6;
            var balanceAfter = 7;
            var ibanOtherParty = 8;
            var nameOtherParty = 9;
            var authorizationCode = 16;
            int[] descriptionIndices = [19, 20, 21];


            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split("\",\"").ToList();
                values[0] = string.Join("", values[0].Skip(1));
                values[values.Count - 1] = string.Join("", values.Take(values.Count - 1));

                if (values == null)
                {
                    logger.LogWarning("Skipping line somehow. Line is: {Line}", line);
                    continue;
                }

                var transaction = new Transaction
                {
                    Iban = values.ElementAt(iban),
                    Currency = values.ElementAt(currency),
                    FollowNumber = ParseCellToInt(values.ElementAt(followNumber), followNumber),
                    DateTransaction = ParseCellToDate(values.ElementAt(date), date),
                    Amount = ParseCellToDecimal(values.ElementAt(amount), amount),
                    BalanceAfterTransaction = ParseCellToDecimal(values.ElementAt(balanceAfter), balanceAfter),
                    IbanOtherParty = values.ElementAt(ibanOtherParty),
                    NameOtherParty = values.ElementAt(nameOtherParty),
                    AuthorizationCode = values.ElementAt(authorizationCode),
                    Description = string.Join(" ", descriptionIndices.Select(idx => values.ElementAt(idx)))
                };

                transactions.Add(transaction);
            }

            var minDate = transactions.Min(t => t.DateTransaction);
            var maxDate = transactions.Max(t => t.DateTransaction);
            var transactionsInDb = await db.Transactions
                .Where(t => t.DateTransaction >= minDate && t.DateTransaction <= maxDate)
                .Select(t => new Tuple<string, int>(t.Iban, t.FollowNumber))
                .ToListAsync();

            var transactionsNew = transactions
                .Where(t => !transactionsInDb.Contains(new Tuple<string, int>(t.Iban, t.FollowNumber)))
                .ToList();

            await db.Transactions.AddRangeAsync(transactionsNew);
            await db.SaveChangesAsync();

            logger.LogInformation("Saved {amount} transactions ranging from {minDate} to {maxDate} successfully",
                transactionsNew.Count,
                transactions.Min(t => t.DateTransaction),
                transactions.Max(t => t.DateTransaction));

            return new Response(transactionsNew.Count, minDate, maxDate);
        }

        private int ParseCellToInt(string value, int columnIndex)
        {
            if (!int.TryParse(value, out int number))
            {
                logger.LogWarning("Unable to parse number {value} at column {columnIndex} to int", value, columnIndex);
            }

            return number;
        }

        private decimal ParseCellToDecimal(string value, int columnIndex)
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
}
