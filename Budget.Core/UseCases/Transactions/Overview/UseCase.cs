using Azure.Data.Tables;
using Budget.Core.Extensions;
using Budget.Core.Models;

namespace Budget.Core.UseCases.Transactions.Overview;

public class UseCase(TableClient dataAccess)
{
    public async Task<Response> HandleAsync(Request request)
    {
        var iban = request.Iban;
        var date = new DateTime(request.Year, request.Month, 1);
        var dateMin = new DateTime(date.Year, date.Month, 1);
        var dateMax = dateMin.AddMonths(1).AddDays(-1);
        var (year, month, _) = date;
        var (previousYear, previousMonth, _) = date.AddMonths(-1);
        var partitionKeyPrevious = $"{previousYear}-{previousMonth}";
        var partitionKey = $"{year}-{month}";
        IEnumerable<Transaction> transactionsAll = dataAccess
            .Query<Transaction>(e => e.PartitionKey == partitionKey || e.PartitionKey == partitionKeyPrevious);
        var dateFirstOfMonth = new DateTime(previousYear, previousMonth, 1);
        var ibansOrdered = transactionsAll
            .Select(t => t.Iban)
            .GroupBy(i => i)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Count())
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
        var ibans = ibansOrdered;
        var ibanSelected = iban == null || !ibansOrdered.Contains(iban) ? ibansOrdered.FirstOrDefault("") : iban;
        var transactions = transactionsAll.Where(t => t.Iban == ibanSelected && t.PartitionKey == partitionKey)
            .ToList();

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
        var balancePerAccount = new Dictionary<string, decimal>();
        decimal incomeFromOwnAccounts = 0;
        decimal expensesVariable = 0;

        foreach (var transaction in transactionsAll)
        {
            var isLastMonth = transaction.PartitionKey == partitionKeyPrevious;
            var isThisMonth = transaction.PartitionKey == partitionKey;
            var week = transaction.DateTransaction.ToIsoWeekNumber();
            var amount = (decimal)transaction.Amount;

            if (transaction.Iban != ibanSelected) continue;

            if (isLastMonth && transaction.IsIncome && transaction.IsFromOtherParty(ibans) &&
                transaction.CashbackForDate == null)
            {
                incomeLastMonth += amount;
            }

            if (isLastMonth && !transaction.IsIncome && (transaction.IsFixed || transaction.IsFromOwnAccount(ibans)))
            {
                expensesFixedLastMonth += amount;
            }

            if (isThisMonth)
            {
                expensesPerWeek.TryAdd(week, 0);
            }

            if (isThisMonth && transaction.IbanOtherParty != null && transaction.IsFromOwnAccount(ibans))
            {
                balancePerAccount.TryAdd(transaction.IbanOtherParty, 0);
                balancePerAccount[transaction.IbanOtherParty] += amount;
            }

            if (isThisMonth && transaction.IsIncome && transaction.IsFromOwnAccount(ibans) &&
                transaction.CashbackForDate == null)
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
                var cashbackWeek = transaction.CashbackForDate.Value.ToIsoWeekNumber();
                if (!expensesPerWeek.ContainsKey(cashbackWeek))
                {
                    expensesPerWeek.Add(cashbackWeek, 0);
                }

                expensesPerWeek[cashbackWeek] += amount;
            }
        }

        return new Response
        {
            IbanSelected = ibanSelected,
            IbansToSelect = ibansOrdered,
            Date = DateOnly.FromDateTime(date),
            DatePreviousMonth = new DateOnly(previousYear, previousMonth, 1),
            ExpensesFixedLastMonth = expensesFixedLastMonth,
            IncomeLastMonth = incomeLastMonth,
            WeeksInMonth = weeksInMonth,
            ExpensesVariable = expensesVariable,
            ExpensesPerWeek = expensesPerWeek,
            IncomeFromOwnAccounts = incomeFromOwnAccounts,
            TransactionsPerWeek = transactionsPerWeek,
            BalancePerAccount = balancePerAccount
        };
    }
}