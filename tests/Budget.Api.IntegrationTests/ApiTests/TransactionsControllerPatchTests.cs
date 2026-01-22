using System.Net.Http.Json;
using Budget.Application.UseCases.UpdateTransactionCashbackDate;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Api.IntegrationTests.ApiTests;

[TestClass]
public class TransactionsControllerPatchTests : BaseApiTests
{
    [TestMethod]
    public async Task UpdateCashbackForDate_ShouldUpdateDateForCashback()
    {
        // Arrange
        await using var app = await CreateSut(nameof(UpdateCashbackForDate_ShouldUpdateDateForCashback), CancellationToken.None);
        var (client, db) = app;

        var transaction = new Transaction
        {
            FollowNumber = 1,
            Iban = "NL12ABCD3456789012",
            Currency = "EUR",
            Amount = 100,
            DateTransaction = new DateOnly(2023, 1, 1),
            BalanceAfterTransaction = 500,
            Description = "Test transaction",
            CashbackForDate = null,
            User = "testuser"
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(CancellationToken.None);
        db.ChangeTracker.Clear();

        var id = transaction.Id;

        var newCashbackDate = new DateOnly(2023, 1, 15);
        // Act
        var result = await client.PatchAsJsonAsync($"/transactions/{id}/cashback-date", new { CashbackForDate = newCashbackDate.ToString("yyyy-MM-dd") }, cancellationToken: CancellationToken.None);

        // Assert
        result.EnsureSuccessStatusCode();

        var response = await result.Content.ReadFromJsonAsync<UpdateTransactionCashbackDateResponse>(cancellationToken: CancellationToken.None);
        Assert.IsNotNull(response);
        Assert.AreEqual(response!.Id, id);
        Assert.AreEqual(newCashbackDate, response.CashbackForDate);

        var updatedTransaction = await db.Transactions.FirstOrDefaultAsync(t => t.Id == id, CancellationToken.None);
        Assert.AreEqual(newCashbackDate, updatedTransaction!.CashbackForDate);
    }
}
