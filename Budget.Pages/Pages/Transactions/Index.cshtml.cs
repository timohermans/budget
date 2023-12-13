using Azure.Data.Tables;
using Budget.Core.Extensions;
using Budget.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace Budget.Pages.Pages.Transactions;

public class IndexModel(TableClient table, ILogger<IndexModel> logger) : PageModel
{
    private List<string> _ibans = [];
    private DateOnly _previousMonth = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    [DisplayName("Rekening")]
    public required string Iban { get; set; }
    public List<SelectListItem> Ibans { get; set; } = [];
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly DatePreviousMonth => _previousMonth;
    public decimal IncomeLastMonth { get; set; } = 0;
    public decimal ExpensesFixedLastMonth { get; set; } = 0;
    public decimal BudgetAvailable => IncomeLastMonth + ExpensesFixedLastMonth;
    public decimal BudgetPerWeek => WeeksInMonth.Count > 0 ? Math.Floor(BudgetAvailable / WeeksInMonth.Count) : 0;
    public List<int> WeeksInMonth { get; set; } = [];
    public decimal ExpensesVariable { get; set; } = 0;
    public Dictionary<int, decimal> ExpensesPerWeek { get; set; } = [];
    public decimal IncomeFromOwnAccounts { get; set; } = 0;
    public Dictionary<int, List<Transaction>> TransactionsPerWeek { get; set; } = [];

    public async void OnGet(int? year, int? month, string? iban)
    {
        if (year.HasValue && month.HasValue)
        {
            Date = DateOnly.FromDateTime(new DateTime(year.Value, month.Value, 1));
        }
        _previousMonth = Date.AddMonths(-1);

        var dateMin = new DateTime(Date.Year, Date.Month, 1);
        var dateMax = dateMin.AddMonths(1).AddDays(-1);

        var partitionKeyPrevious = $"{_previousMonth.Year}-{_previousMonth.Month}";
        var partitionKey = $"{Date.Year}-{Date.Month}";
        var transactionsAll = table.Query<Transaction>(e => e.PartitionKey == partitionKey || e.PartitionKey == partitionKeyPrevious).ToList();
        SetupIbans(transactionsAll, iban);
        var transactions = transactionsAll.Where(t => t.Iban == Iban && t.PartitionKey == partitionKey).ToList();
        var transactionsLastMonth = transactionsAll.Where(t => t.Iban == Iban && t.PartitionKey == partitionKeyPrevious).ToList();

        CalculateWeekInfo(dateMax);

        AggregatePreviousMonthData(transactionsLastMonth);
        foreach (var transaction in transactions)
        {
            var week = transaction.DateTransaction.ToIsoWeekNumber();
            var amount = (decimal)transaction.Amount;
            if (!ExpensesPerWeek.ContainsKey(week))
            {
                ExpensesPerWeek.Add(week, 0);
            }

            if (transaction.IsIncome && transaction.IsFromOwnAccount(_ibans))
            {
                IncomeFromOwnAccounts += amount;
            }

            if (!transaction.IsIncome && !transaction.IsFixed && transaction.IsFromOtherParty(_ibans))
            {
                ExpensesVariable += amount;
                ExpensesPerWeek[week] += amount;
            }
        }

        TransactionsPerWeek = transactions
            .GroupBy(t => t.DateTransaction.ToIsoWeekNumber())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

        logger.LogInformation("Load {Iban}'s transactions of {Date}", Iban, Date);
    }

    private void SetupIbans(List<Transaction> transactionsAll, string? iban)
    {
        var dateFirstOfMonth = new DateTime(_previousMonth.Year, _previousMonth.Month, 1);
        var ibansOrdered = transactionsAll
                .Where(t => t.DateTransaction >= dateFirstOfMonth && t.DateTransaction < dateFirstOfMonth.AddMonths(2))
                .Select(t => t.Iban)
                .GroupBy(i => i)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Count())
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

        _ibans = ibansOrdered;
        Iban = iban == null || !ibansOrdered.Contains(iban) ? ibansOrdered.FirstOrDefault("") : iban;
        Ibans = ibansOrdered.Select(iban => new SelectListItem(iban, iban, iban == Iban)).ToList();
    }

    private void AggregatePreviousMonthData(List<Transaction> transactionsLastMonth)
    {
        foreach (var transaction in transactionsLastMonth)
        {
            var amount = (decimal)transaction.Amount;
            if (transaction.IsIncome && transaction.IsFromOtherParty(_ibans))
            {
                IncomeLastMonth += amount;
            }

            if (!transaction.IsIncome && (transaction.IsFixed || transaction.IsFromOwnAccount(_ibans)))
            {
                ExpensesFixedLastMonth += amount;
            }
        }
    }

    public void CalculateWeekInfo(DateTime dateMax)
    {
        WeeksInMonth = Enumerable.Range(0, dateMax.Day)
            .Select(day => new DateTime(dateMax.Year, dateMax.Month, day + 1).ToIsoWeekNumber())
            .Distinct()
            .ToList();
    }
}