using System.Globalization;
using Budget.Application.UseCases.TransactionsFileEtl;
using CsvHelper;
using CsvHelper.Configuration;

namespace Budget.E2eTests.Helpers;

public class TransactionsCsvFileBuilder : IAsyncDisposable
{
    private readonly List<TransactionsFileCsvMap> _records = new();
    private readonly string _filePath;

    public TransactionsCsvFileBuilder()
    {
        var fileName = $"transactions-{Guid.NewGuid()}.csv";
        _filePath = Path.Combine(Path.GetTempPath(), fileName);
    }

    public TransactionsCsvFileBuilder AddRecord(TransactionsFileCsvMap record)
    {
        _records.Add(record);
        return this;
    }

    public TransactionsCsvFileBuilder AddRecords(IEnumerable<TransactionsFileCsvMap> records)
    {
        _records.AddRange(records);
        return this;
    }

    public async Task<string> BuildAsync()
    {
        await using (var writer = new StreamWriter(_filePath))
        await using (var csv = new CsvWriter(writer, new CsvConfiguration(new CultureInfo("nl-NL")) { Delimiter = "," }))
        {
            await csv.WriteRecordsAsync(_records);
        }

        return _filePath;
    }

    public async ValueTask DisposeAsync()
    {
        if (File.Exists(_filePath))
        {
            try 
            {
                File.Delete(_filePath);
            }
            catch 
            {
                // Best effort deletion
            }
        }
    }
}
