using Budget.Core.Extensions;
using Budget.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace Budget.Pages.Pages.Transactions;

public class IndexModel(BudgetContext db, ILogger<IndexModel> logger) : PageModel
{
    private List<string> _ibans = new List<string>();
    private DateOnly _previousMonth = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty]
    [DisplayName("Rekening")]
    public required string Iban { get; set; }
    public List<SelectListItem> Ibans { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public decimal IncomeLastMonth { get; set; } = 0;
    public decimal ExpensesFixedLastMonth { get; set; } = 0;
    public decimal BudgetPerWeek { get; set; } = 0;

    public void OnGet(int? year, int? month, string? iban)
    {
        if (year.HasValue && month.HasValue)
        {
            Date = DateOnly.FromDateTime(new DateTime(year.Value, month.Value, 1));
        }
        _previousMonth = Date.AddMonths(-1);

        SetupIbans(iban);
        AggregatePreviousMonthData();

        // TODO: aantal weken december berekenen en hardcoded zooi weghalen
        // TODO: hardcoded november/decmeber titel weghalen





        logger.LogInformation("Load {Iban}'s transactions of {Date}", Iban, Date);
    }

    private void SetupIbans(string? iban)
    {
        var dateFirstOfMonth = new DateOnly(_previousMonth.Year, _previousMonth.Month, 1);
        var ibansOrdered = db.Transactions
                .Where(t => t.DateTransaction >= dateFirstOfMonth && t.DateTransaction < dateFirstOfMonth.AddMonths(2))
                .Select(t => t.Iban)
                .GroupBy(i => i)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Count())
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

        _ibans = ibansOrdered;
        Iban = iban == null || !ibansOrdered.Contains(iban) ? ibansOrdered.First() : iban;
        Ibans = ibansOrdered.Select(iban => new SelectListItem(iban, iban, iban == Iban)).ToList();
    }

    private void AggregatePreviousMonthData()
    {
        var monthPrevious = new DateOnly(Date.AddMonths(-1).Year, Date.AddMonths(-1).Month, 1);
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
}