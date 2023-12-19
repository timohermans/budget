﻿using Azure.Data.Tables;
using Budget.Core.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Budget.Core.UseCases
{
    public class TransactionFileUploadUseCase(TableClient db, ILogger<TransactionFileUploadUseCase> logger)
    {
        public record Response(int AmountInserted, DateTime DateMin, DateTime DateMax);

        public Response Handle(Stream stream)
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
                var values = line.Split("\",\"").ToList();
                values[0] = string.Join("", values[0].Skip(1));
                values[values.Count - 1] = string.Join("", values.Take(values.Count - 1));

                if (values == null)
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
                    Description = string.Join(" ", descriptionIndices.Select(idx => values.ElementAt(idx)))
                };

                transactions.Add(transaction);
            }

            var transactionsPerPartitionKey = transactions
                .GroupBy(t => t.PartitionKey)
                .ToDictionary(kvp => kvp.Key, g => g.ToList());

            foreach (var partition in transactionsPerPartitionKey)
            {
                Parallel.ForEach(partition.Value.Chunk(100), async transactions =>
                {
                    List<TableTransactionAction> addEntitiesBatch = transactions
                        .Select(transaction => new TableTransactionAction(TableTransactionActionType.UpsertMerge, transaction))
                        .ToList();

                    var response = await db.SubmitTransactionAsync(addEntitiesBatch);

                    logger.LogInformation("Saved {amount} transactions out of {count} in current batch", response.Value.Count(r => r.Status == 204), transactions.Count());
                });
            }


            var minDate = transactions.Min(t => t.DateTransaction);
            var maxDate = transactions.Max(t => t.DateTransaction);

            return new Response(transactions.Count, minDate, maxDate);
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
