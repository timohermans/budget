using Budget.Core.DataAccess;
using Budget.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Budget.Core.UseCases.Transactions.Overview;

public class UseCase(BudgetContext dataAccess)
{
    public async Task<Response> HandleAsync(Request request)
    {
        var iban = request.Iban;
        var date = new DateOnly(request.Year, request.Month, 1);
        var dateMin = new DateOnly(date.Year, date.Month, 1);
        var dateMax = dateMin.AddMonths(1).AddDays(-1);
        var (year, month, _) = date;
        var previousMonthDate = date.AddMonths(-1);
        var (previousYear, previousMonth, _) = previousMonthDate;
        var transactionsAll = await dataAccess.Transactions
            .Where(t => t.DateTransaction >= previousMonthDate && t.DateTransaction < date.AddMonths(1))
            .OrderBy(t => t.DateTransaction)
            .ToListAsync();

        var ibansOrdered = transactionsAll
            .Select(t => t.Iban)
            .GroupBy(i => i)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Count())
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
        var ibans = ibansOrdered;
        var ibanSelected = iban == null || !ibansOrdered.Contains(iban) ? ibansOrdered.FirstOrDefault("") : iban;
        var transactions = transactionsAll.Where(t => t.Iban == ibanSelected && t.DateTransaction.Year == year && t.DateTransaction.Month == month)
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
            var isLastMonth = transaction.DateTransaction.Year == previousYear &&
                              transaction.DateTransaction.Month == previousMonth;
            var isThisMonth = transaction.DateTransaction.Year == year && transaction.DateTransaction.Month == month;
            var week = transaction.DateTransaction.ToIsoWeekNumber();
            var amount = transaction.Amount;

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

            if (isThisMonth && transaction is { IsIncome: false, IsFixed: false } && transaction.IsFromOtherParty(ibans))
            {
                expensesVariable += amount;
                expensesPerWeek[week] += amount;
            }

            if (isThisMonth && transaction is { IsIncome: true, CashbackForDate: not null })
            {
                expensesVariable += amount;
                var cashbackWeek = transaction.CashbackForDate.Value.ToIsoWeekNumber();
                
                expensesPerWeek.TryAdd(cashbackWeek, 0);
                expensesPerWeek[cashbackWeek] += amount;
            }
        }

        decimal budgetAvailable = incomeLastMonth + expensesFixedLastMonth;
        return new Response
        {
            IbanSelected = ibanSelected,
            IbansToSelect = ibansOrdered,
            Date = date,
            DatePreviousMonth = new DateOnly(previousYear, previousMonth, 1),
            ExpensesFixedLastMonth = expensesFixedLastMonth,
            IncomeLastMonth = incomeLastMonth,
            WeeksInMonth = weeksInMonth,
            ExpensesVariable = expensesVariable,
            ExpensesPerWeek = expensesPerWeek,
            IncomeFromOwnAccounts = incomeFromOwnAccounts,
            TransactionsPerWeek = transactionsPerWeek,
            BalancePerAccount = balancePerAccount,
            BudgetAvailable = budgetAvailable,
            BudgetPerWeek = weeksInMonth.Count > 0 ? Math.Floor(budgetAvailable / weeksInMonth.Count) : 0
        };
    }
}