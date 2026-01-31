using System.Globalization;
using CsvHelper.Configuration;

namespace Budget.Application.Settings;

public class CsvSettings
{
    public static CsvConfiguration BudgetCsvConfig =>
        new(new CultureInfo("nl-NL"))
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Quote = '"',
            ShouldQuote = _ => true
        };
}