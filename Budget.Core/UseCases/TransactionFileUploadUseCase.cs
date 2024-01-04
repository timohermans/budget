using Azure.Data.Tables;
using Budget.Core.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Budget.Core.UseCases
{
    public class TransactionFileUploadUseCase(TableClient db, ILogger<TransactionFileUploadUseCase> logger)
    {
        public record Response(int AmountInserted, DateTime DateMin, DateTime DateMax);

        public async Task<Response> HandleAsync(Stream stream)
        {
            logger.LogInformation("Handling Transaction file upload");

            List<Transaction> transactions = new List<Transaction>();

            using var reader = new StreamReader(stream);

            reader.ReadLine();

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


            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var values = SplitLine(line).ToList();

                if (values.Count == 0)
                {
                    logger.LogWarning("Skipping line somehow. Line is: {Line}", line);
                    continue;
                }

                var iban = values.ElementAt(ibanIndex);
                var followNumber = values.ElementAt(followNumberIndex);
                var dateTransaction = ParseCellToDate(values.ElementAt(date), date);
                var transaction = new Transaction
                {
                    RowKey = $"{iban}-{followNumber}",
                    PartitionKey = $"{dateTransaction.Year}-{dateTransaction.Month}",
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

            var transactionsPerPartitionKey = transactions
                .GroupBy(t => t.PartitionKey)
                .ToDictionary(kvp => kvp.Key, g => g.ToList());

            foreach (var partition in transactionsPerPartitionKey)
            {
                await Parallel.ForEachAsync(partition.Value.Chunk(100), async (transactionsChunk, cancelToken) =>
                {
                    List<TableTransactionAction> addEntitiesBatch = transactionsChunk
                        .Select(transaction =>
                            new TableTransactionAction(TableTransactionActionType.UpsertMerge, transaction))
                        .ToList();

                    var response = await db.SubmitTransactionAsync(addEntitiesBatch, cancelToken);

                    logger.LogInformation("Saved {amount} transactions out of {count} in current batch",
                        response.Value.Count(r => r.Status == 204), transactionsChunk.Count());
                });
            }

            var minDate = transactions.Min(t => t.DateTransaction);
            var maxDate = transactions.Max(t => t.DateTransaction);

            return new Response(transactions.Count, minDate, maxDate);
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
                    throw new ArgumentException("Unable to parse line at index " + i + " with value " + line[i] + " in line " + line);
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

        private double ParseCellToDouble(string value, int columnIndex)
        {
            string doubleInput = value.Replace(",", ".");
            if (!double.TryParse(doubleInput, CultureInfo.InvariantCulture, out double number))
            {
                logger.LogWarning("Unable to parse number {value} at column {columnIndex} to int", value, columnIndex);
            }

            return number;
        }

        private DateTime ParseCellToDate(string value, int columnIndex)
        {
            if (!DateTime.TryParse(value, out var date))
            {
                logger.LogWarning("Unable to parse {value} at {columnIndex} to date", value, columnIndex);
            }

            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
    }
}