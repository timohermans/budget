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
    private DateOnly _previousMonth = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    [DisplayName("Rekening")]
    public required string Iban { get; set; }
    public List<SelectListItem> IbansToSelect { get; set; } = [];
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
        var dateFirstOfMonth = new DateTime(_previousMonth.Year, _previousMonth.Month, 1);
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

        foreach (var transaction in transactionsAll)
        {
            var isLastMonth = transaction.PartitionKey == partitionKeyPrevious;
            var isThisMonth = transaction.PartitionKey == partitionKey;
            var week = transaction.DateTransaction.ToIsoWeekNumber();
            var amount = (decimal)transaction.Amount;

            if (transaction.Iban != ibanSelected) continue;

            if (isLastMonth && transaction.IsIncome && transaction.IsFromOtherParty(ibans))
            {
                IncomeLastMonth += amount;
            }

            if (isLastMonth && !transaction.IsIncome && (transaction.IsFixed || transaction.IsFromOwnAccount(ibans)))
            {
                ExpensesFixedLastMonth += amount;
            }

            if (isThisMonth && !ExpensesPerWeek.ContainsKey(week))
            {
                ExpensesPerWeek.Add(week, 0);
            }

            if (isThisMonth && transaction.IsIncome && transaction.IsFromOwnAccount(ibans))
            {
                IncomeFromOwnAccounts += amount;
            }

            if (isThisMonth && !transaction.IsIncome && !transaction.IsFixed && transaction.IsFromOtherParty(ibans))
            {
                ExpensesVariable += amount;
                ExpensesPerWeek[week] += amount;
            }
        }

        Iban = ibanSelected;
        IbansToSelect = ibansOrdered.Select(iban => new SelectListItem(iban, iban, iban == ibanSelected)).ToList();
        WeeksInMonth = Enumerable.Range(0, dateMax.Day)
            .Select(day => new DateTime(dateMax.Year, dateMax.Month, day + 1).ToIsoWeekNumber())
            .Distinct()
            .ToList();
        TransactionsPerWeek = transactions
            .GroupBy(t => t.DateTransaction.ToIsoWeekNumber())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

        logger.LogInformation("Load {Iban}'s transactions of {Date}", this.Iban, Date);
    }
}