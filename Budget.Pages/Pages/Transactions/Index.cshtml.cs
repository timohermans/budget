using Budget.Core.Extensions;
using Budget.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Budget.Pages.Pages.Transactions;

public class IndexModel(BudgetContext db, ILogger<IndexModel> logger) : PageModel
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

    public void OnGet(int? year, int? month, string? iban)
    {
        if (year.HasValue && month.HasValue)
        {
            Date = DateOnly.FromDateTime(new DateTime(year.Value, month.Value, 1));
        }
        _previousMonth = Date.AddMonths(-1);

        SetupIbans(iban);
        AggregatePreviousMonthData();

        var dateMin = new DateTime(Date.Year, Date.Month, 1);
        var dateMax = dateMin.AddMonths(1).AddDays(-1);

        var transactions = db.Transactions
            .Where(t => t.DateTransaction >= dateMin && t.DateTransaction <= dateMax && t.Iban == Iban)
            .ToList();

        CalculateWeekInfo(dateMax);

        foreach (var transaction in transactions)
        {
            var week = transaction.DateTransaction.ToIsoWeekNumber();
            if (!ExpensesPerWeek.ContainsKey(week))
            {
                ExpensesPerWeek.Add(week, 0);
            }

            if (transaction.IsIncome && transaction.IsFromOwnAccount(_ibans))
            {
                IncomeFromOwnAccounts += transaction.Amount;
            }

            if (!transaction.IsIncome && !transaction.IsFixed && transaction.IsFromOtherParty(_ibans))
            {
                ExpensesVariable += transaction.Amount;
                ExpensesPerWeek[week] += transaction.Amount;
            }
        }

        TransactionsPerWeek = transactions
            .GroupBy(t => t.DateTransaction.ToIsoWeekNumber())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

        logger.LogInformation("Load {Iban}'s transactions of {Date}", Iban, Date);
    }

    private void SetupIbans(string? iban)
    {
        var dateFirstOfMonth = new DateTime(_previousMonth.Year, _previousMonth.Month, 1);
        var ibansOrdered = db.Transactions
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

    private void AggregatePreviousMonthData()
    {
        var monthPrevious = new DateTime(Date.AddMonths(-1).Year, Date.AddMonths(-1).Month, 1);
        var transactionsLastMonth = db.Transactions
            .Where(t => t.DateTransaction >= monthPrevious
            && t.DateTransaction < monthPrevious.AddMonths(1)
            && t.Iban == Iban)
            .ToList();

        foreach (var transaction in transactionsLastMonth)
        {
            if (transaction.IsIncome && transaction.IsFromOtherParty(_ibans))
            {
                IncomeLastMonth += transaction.Amount;
            }

            if (!transaction.IsIncome && (transaction.IsFixed || transaction.IsFromOwnAccount(_ibans)))
            {
                ExpensesFixedLastMonth += transaction.Amount;
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