using Azure.Data.Tables;
using Budget.Core.Extensions;
using Budget.Core.Models;

namespace Budget.Core.UseCases;

public class TransactionGetOverviewUseCase(TableClient table)
{
    public class Request
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public string? Iban { get; init; }
    }

    public class Response
    {
        public required string IbanSelected { get; init; }
        public required IEnumerable<string> IbansToSelect { get; init; }
        public required DateOnly Date { get; init; }
        public required DateOnly DatePreviousMonth { get; init; }
        public required decimal ExpensesFixedLastMonth { get; init; }
        public required decimal IncomeLastMonth { get; init; }
        public required List<int> WeeksInMonth { get; init; }
        public required decimal ExpensesVariable { get; init; }
        public required Dictionary<int, decimal> ExpensesPerWeek { get; init; }
        public required decimal IncomeFromOwnAccounts { get; init; }
        public required Dictionary<int, List<Transaction>> TransactionsPerWeek { get; init; }
    }

    public Response Handle(Request request)
    {
        var year = request.Year;
        var month = request.Month;
        var iban = request.Iban;
        var date = new DateTime(year, month, 1);
        var previousMonth = date.AddMonths(-1);
        var dateMin = new DateTime(date.Year, date.Month, 1);
        var dateMax = dateMin.AddMonths(1).AddDays(-1);
        var partitionKeyPrevious = $"{previousMonth.Year}-{previousMonth.Month}";
        var partitionKey = $"{date.Year}-{date.Month}";
        var transactionsAll = table.Query<Transaction>(e => e.PartitionKey == partitionKey || e.PartitionKey == partitionKeyPrevious).ToList();
        var dateFirstOfMonth = new DateTime(previousMonth.Year, previousMonth.Month, 1);
        var ibansOrdered = transactionsAll
                .Select(t => t.Iban)
                .GroupBy(i => i)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Count())
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        var ibans = ibansOrdered;
        var ibanSelected = iban == null || !ibansOrdered.Contains(iban) ? ibansOrdered.FirstOrDefault("") : iban;
        var transactions = transactionsAll.Where(t => t.Iban == ibanSelected && t.PartitionKey == partitionKey).ToList();

        var weeksInMonth = Enumerable.Range(0, dateMax.Day)
            .Select(day => new DateTime(dateMax.Year, dateMax.Month, day + 1).ToIsoWeekNumber())
            .Distinct()
            .ToList();

        var transactionsPerWeek = transactions
            .GroupBy(t => t.DateTransaction.ToIsoWeekNumber())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

        decimal incomeLastMonth = 0;
        decimal expensesFixedLastMonth = 0;
        var expensesPerWeek = new Dictionary<int, decimal>();
        decimal incomeFromOwnAccounts = 0;
        decimal expensesVariable = 0;

        foreach (var transaction in transactionsAll)
        {
            var isLastMonth = transaction.PartitionKey == partitionKeyPrevious;
            var isThisMonth = transaction.PartitionKey == partitionKey;
            var week = transaction.DateTransaction.ToIsoWeekNumber();
            var amount = (decimal)transaction.Amount;

            if (transaction.Iban != ibanSelected) continue;

            if (isLastMonth && transaction.IsIncome && transaction.IsFromOtherParty(ibans) && transaction.CashbackForDate == null)
            {
                incomeLastMonth += amount;
            }

            if (isLastMonth && !transaction.IsIncome && (transaction.IsFixed || transaction.IsFromOwnAccount(ibans)))
            {
                expensesFixedLastMonth += amount;
            }

            if (isThisMonth && !expensesPerWeek.ContainsKey(week))
            {
                expensesPerWeek.Add(week, 0);
            }

            if (isThisMonth && transaction.IsIncome && transaction.IsFromOwnAccount(ibans) && transaction.CashbackForDate == null)
            {
                incomeFromOwnAccounts += amount;
            }

            if (isThisMonth && !transaction.IsIncome && !transaction.IsFixed && transaction.IsFromOtherParty(ibans))
            {
                expensesVariable += amount;
                expensesPerWeek[week] += amount;
            }
            
            if (isThisMonth && transaction.IsIncome && transaction.CashbackForDate != null)
            {
                expensesVariable += amount;
                expensesPerWeek[transaction.CashbackForDate.Value.ToIsoWeekNumber()] += amount;
            }
        }

        return new Response
        {
            IbanSelected = ibanSelected,
            IbansToSelect = ibansOrdered,
            Date = DateOnly.FromDateTime(date),
            DatePreviousMonth = DateOnly.FromDateTime(previousMonth),
            ExpensesFixedLastMonth = expensesFixedLastMonth,
            IncomeLastMonth = incomeLastMonth,
            WeeksInMonth = weeksInMonth,
            ExpensesVariable = expensesVariable,
            ExpensesPerWeek = expensesPerWeek,
            IncomeFromOwnAccounts = incomeFromOwnAccounts,
            TransactionsPerWeek = transactionsPerWeek
        };
    }

}
