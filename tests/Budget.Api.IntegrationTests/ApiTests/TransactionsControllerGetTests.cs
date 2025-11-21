using System.Net.Http.Json;
using Budget.Api.Models;
using Budget.Domain.Contracts;
using Budget.Domain.Entities;

namespace Budget.Api.IntegrationTests.ApiTests;

[TestClass]
public class TransactionsControllerGetTests : BaseApiTests
{
    [TestMethod]
    public async Task GetTransactions_IncludesAndExcludesCorrectDates()
    {
        // Arrange
        await using var app = await CreateSut(nameof(GetTransactions_IncludesAndExcludesCorrectDates), CancellationToken.None);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
                new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
                new Transaction { Id = 2, FollowNumber = 2, Iban = "NL01TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
                new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 4, 1), BalanceAfterTransaction = 600 }
        };

        db.Transactions.AddRange(transactions);
        db.SaveChanges();

        // Act
        var result = await client.GetAsync($"/transactions?startDate={new DateOnly(2025, 3, 1).ToString("yyyy-MM-dd")}&endDate={new DateOnly(2025, 3, 31).ToString("yyyy-MM-dd")}", CancellationToken.None);

        result.EnsureSuccessStatusCode();

        // Assert
        var returnedTransactions = await result.Content.ReadFromJsonAsync<List<TransactionResponseModel>>(CancellationToken.None);
        Assert.IsNotNull(returnedTransactions);
        Assert.AreEqual(2, returnedTransactions.Count);
        Assert.IsTrue(returnedTransactions.Any(t => t.Id == 1));
        Assert.IsTrue(returnedTransactions.Any(t => t.Id == 2));
        Assert.IsFalse(returnedTransactions.Any(t => t.Id == 3));
    }

    [TestMethod]
    public async Task GetTransactions_FiltersByIban()
    {
        // Arrange
        await using var app = await CreateSut(nameof(GetTransactions_IncludesAndExcludesCorrectDates), CancellationToken.None);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
            new Transaction { Id = 4, FollowNumber = 4, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 4, 3), BalanceAfterTransaction = 600 }
        };

        await db.Transactions.AddRangeAsync(transactions, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await client.GetAsync($"/transactions?startDate={new DateOnly(2025, 3, 1).ToString("yyyy-MM-dd")}&endDate={new DateOnly(2025, 3, 31).ToString("yyyy-MM-dd")}&iban=NL01TEST", CancellationToken.None);

        result.EnsureSuccessStatusCode();

        var returnedTransactions = await result.Content.ReadFromJsonAsync<List<TransactionResponseModel>>(CancellationToken.None);

        // Assert
        Assert.IsNotNull(returnedTransactions);
        Assert.AreEqual(2, returnedTransactions.Count);
        Assert.IsTrue(returnedTransactions.Any(t => t.Id == 1));
        Assert.IsTrue(returnedTransactions.Any(t => t.Id == 3));
        Assert.IsFalse(returnedTransactions.Any(t => t.Id == 2));
        Assert.IsFalse(returnedTransactions.Any(t => t.Id == 4));
    }

    [TestMethod]
    public async Task GetAllDistinctIbans_ReturnsDistinctIbans()
    {
        // Arrange
        await using var app = await CreateSut(nameof(GetTransactions_IncludesAndExcludesCorrectDates), CancellationToken.None);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 }
        };

        await db.Transactions.AddRangeAsync(transactions, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await client.GetAsync("/transactions/ibans", CancellationToken.None);

        // Assert
        var ibansResult = await result.Content.ReadFromJsonAsync<IEnumerable<string>>(cancellationToken: CancellationToken.None);

        Assert.IsNotNull(ibansResult);
        Assert.AreEqual(2, ibansResult.Count());
        Assert.IsTrue(ibansResult.Contains("NL01TEST"));
        Assert.IsTrue(ibansResult.Contains("NL02TEST"));
    }

    [TestMethod]
    public async Task GetAllDistinctIbans_ReturnsDistinctIbansOrderedByFrequency()
    {
        // Arrange
        await using var app = await CreateSut(nameof(GetTransactions_IncludesAndExcludesCorrectDates), CancellationToken.None);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
            new Transaction { Id = 4, FollowNumber = 4, Iban = "NL03TEST", Currency = "EUR", Amount = 400, DateTransaction = new DateOnly(2025, 3, 4), BalanceAfterTransaction = 1000 },
            new Transaction { Id = 5, FollowNumber = 5, Iban = "NL01TEST", Currency = "EUR", Amount = 500, DateTransaction = new DateOnly(2025, 3, 5), BalanceAfterTransaction = 1500 }
        };

        await db.Transactions.AddRangeAsync(transactions, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await client.GetAsync("/transactions/ibans", CancellationToken.None);

        // Assert
        result.EnsureSuccessStatusCode();
        var returnedIbans = await result.Content.ReadFromJsonAsync<IEnumerable<string>>(cancellationToken: CancellationToken.None);

        Assert.IsNotNull(returnedIbans);
        Assert.AreEqual(3, returnedIbans.Count());
        Assert.AreEqual("NL01TEST", returnedIbans.ElementAt(0));
        Assert.AreEqual("NL02TEST", returnedIbans.ElementAt(1));
        Assert.AreEqual("NL03TEST", returnedIbans.ElementAt(2));
    }

    [TestMethod]
    public async Task GetCashFlowPerIbanAsync_ReturnsCorrectCashflow()
    {
        // Arrange
        await using var app = await CreateSut(nameof(GetTransactions_IncludesAndExcludesCorrectDates), CancellationToken.None);
        var (client, db) = app;

        var transactions = new List<Transaction>
        {
            new Transaction { Id = 1, FollowNumber = 1, Iban = "NL01TEST", Currency = "EUR", Amount = 100, DateTransaction = new DateOnly(2025, 3, 1), BalanceAfterTransaction = 100 },
            new Transaction { Id = 2, FollowNumber = 2, Iban = "NL02TEST", Currency = "EUR", Amount = 200, DateTransaction = new DateOnly(2025, 3, 2), BalanceAfterTransaction = 300 },
            new Transaction { Id = 3, FollowNumber = 3, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 600 },
            new Transaction { Id = 4, FollowNumber = 4, Iban = "NL01TEST", Currency = "EUR", Amount = 300, DateTransaction = new DateOnly(2025, 3, 3), BalanceAfterTransaction = 400 },
            new Transaction { Id = 5, FollowNumber = 5, Iban = "NL03TEST", Currency = "EUR", Amount = 400, DateTransaction = new DateOnly(2025, 3, 4), BalanceAfterTransaction = 1000 },
            new Transaction { Id = 6, FollowNumber = 6, Iban = "NL01TEST", Currency = "EUR", Amount = 500, DateTransaction = new DateOnly(2025, 3, 5), BalanceAfterTransaction = 1500 }
        };

        await db.Transactions.AddRangeAsync(transactions, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        // Act
        var actionResult = await client.GetAsync($"/transactions/cashflow-per-iban?startDate={new DateOnly(2025, 3, 1).ToString("yyyy-MM-dd")}&endDate={new DateOnly(2025, 3, 31).ToString("yyyy-MM-dd")}", CancellationToken.None);

        // Assert
        actionResult.EnsureSuccessStatusCode();
        var result = await actionResult.Content.ReadFromJsonAsync<CashflowDto>(cancellationToken: CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual("NL01TEST", result.Iban);
        Assert.AreEqual(3, result.BalancesPerDate.Count());
        var balances = result.BalancesPerDate.ToList();
        Assert.AreEqual(new DateOnly(2025, 3, 1), balances[0].Date);
        Assert.AreEqual(100, balances[0].Balance);
        Assert.AreEqual(new DateOnly(2025, 3, 3), balances[1].Date);
        Assert.AreEqual(400, balances[1].Balance);
        Assert.AreEqual(new DateOnly(2025, 3, 5), balances[2].Date);
        Assert.AreEqual(1500, balances[2].Balance);
    }
}
