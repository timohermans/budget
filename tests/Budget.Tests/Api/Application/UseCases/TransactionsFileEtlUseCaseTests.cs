using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain.Entities;
using Budget.Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Budget.Tests.Api.Application.UseCases;

[TestClass]
public class TransactionsFileEtlUseCaseTests(TestContext testContext) : BaseApiTests(testContext)
{
    [TestMethod]
    public async Task HandleAsync_SingleTransaction_Success()
    {
        // Arrange
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(
            "\"IBAN/BBAN\",\"Munt\",\"BIC\",\"Volgnr\",\"Datum\",\"Rentedatum\",\"Bedrag\",\"Saldo na trn\",\"Tegenrekening IBAN/BBAN\",\"Naam tegenpartij\",\"Naam uiteindelijke partij\",\"Naam initi\ufffdrende partij\",\"BIC tegenpartij\",\"Code\",\"Batch ID\",\"Transactiereferentie\",Machtigingskenmerk,\"Incassant ID\",\"Betalingskenmerk\",\"Omschrijving-1\",\"Omschrijving-2\",\"Omschrijving-3\",\"Reden retour\",\"Oorspr bedrag\",\"Oorspr munt\",\"Koers\"");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012107\",\"2023-11-20\",\"2023-11-20\",\"+4000,00\",\"+4000,00\",\"NL11INGB00022222\",\"Werkgever 1\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423687\",,,,\"Salaris 1\",\" \",,,,,\n");
        await writer.FlushAsync();
        stream.Position = 0;

        var username = CreateUniqueUserName("user1");
        await using var db = await CreateContext(username);

        var useCase = new TransactionsFileEtlUseCase(new TransactionRepository(db),
            NullLogger<TransactionsFileEtlUseCase>.Instance);

        // Act
        var result = await useCase.HandleAsync(stream, username);

        // Assert
        db.ChangeTracker.Clear();
        var transactions = await db.Transactions.ToListAsync(TestContext.CancellationTokenSource.Token);
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(1, transactions);
        var transaction = transactions.FirstOrDefault();
        Assert.IsNotNull(transaction);
        Assert.AreEqual(12107, transaction.FollowNumber);
        Assert.AreEqual("NL11RABO0104946666", transaction.Iban);
        Assert.AreEqual(new DateOnly(2023, 11, 20), transaction.DateTransaction);
        Assert.AreEqual("EUR", transaction.Currency);
        Assert.AreEqual(4000, transaction.Amount);
        Assert.AreEqual(4000, transaction.BalanceAfterTransaction);
        Assert.AreEqual("NL11INGB00022222", transaction.IbanOtherParty);
        Assert.AreEqual("Werkgever 1", transaction.NameOtherParty);
        Assert.IsNotNull(transaction.AuthorizationCode);
        Assert.AreEqual(0, transaction.AuthorizationCode.Length);
        Assert.AreEqual("Salaris 1", transaction.Description);
    }

    [TestMethod]
    public async Task HandleAsync_MultipleTransactions_Success()
    {
        // Arrange
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(
            "\"IBAN/BBAN\",\"Munt\",\"BIC\",\"Volgnr\",\"Datum\",\"Rentedatum\",\"Bedrag\",\"Saldo na trn\",\"Tegenrekening IBAN/BBAN\",\"Naam tegenpartij\",\"Naam uiteindelijke partij\",\"Naam initi\ufffdrende partij\",\"BIC tegenpartij\",\"Code\",\"Batch ID\",\"Transactiereferentie\",Machtigingskenmerk,\"Incassant ID\",\"Betalingskenmerk\",\"Omschrijving-1\",\"Omschrijving-2\",\"Omschrijving-3\",\"Reden retour\",\"Oorspr bedrag\",\"Oorspr munt\",\"Koers\"");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012107\",\"2023-11-20\",\"2023-11-20\",\"+4000,00\",\"+4000,00\",\"NL11INGB00022222\",\"Werkgever 1\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423687\",,,,\"Salaris 1\",\" \",,,,,\n");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012108\",\"2023-11-21\",\"2023-11-21\",\"-2000,00\",\"+4000,00\",\"NL11INGB00033333\",\"Werkgever 2\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423688\",,,,\"Salaris 2\",\" \",,,,,\n");
        await writer.FlushAsync();
        stream.Position = 0;

        var username = CreateUniqueUserName("user1");
        await using var db = await CreateContext(username);

        var useCase = new TransactionsFileEtlUseCase(new TransactionRepository(db),
            NullLogger<TransactionsFileEtlUseCase>.Instance);

        // Act
        var result = await useCase.HandleAsync(stream, username);

        // Assert
        db.ChangeTracker.Clear();
        var transactions = await db.Transactions.ToListAsync(TestContext.CancellationTokenSource.Token);
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, transactions);
        var transaction1 = transactions.FirstOrDefault(t => t.FollowNumber == 12107);
        var transaction2 = transactions.FirstOrDefault(t => t.FollowNumber == 12108);
        Assert.IsNotNull(transaction1);
        Assert.IsNotNull(transaction2);
        Assert.AreEqual("NL11RABO0104946666", transaction1.Iban);
        Assert.AreEqual(new DateOnly(2023, 11, 20), transaction1.DateTransaction);
        Assert.AreEqual("EUR", transaction1.Currency);
        Assert.AreEqual(4000, transaction1.Amount);
        Assert.AreEqual(4000, transaction1.BalanceAfterTransaction);
        Assert.AreEqual("NL11INGB00022222", transaction1.IbanOtherParty);
        Assert.AreEqual("Werkgever 1", transaction1.NameOtherParty);
        Assert.IsNotNull(transaction1.AuthorizationCode);
        Assert.AreEqual(0, transaction1.AuthorizationCode.Length);
        Assert.AreEqual("Salaris 1", transaction1.Description);

        Assert.AreEqual("NL11RABO0104946666", transaction2.Iban);
        Assert.AreEqual(new DateOnly(2023, 11, 21), transaction2.DateTransaction);
        Assert.AreEqual("EUR", transaction2.Currency);
        Assert.AreEqual(-2000, transaction2.Amount);
        Assert.AreEqual(4000, transaction2.BalanceAfterTransaction);
        Assert.AreEqual("NL11INGB00033333", transaction2.IbanOtherParty);
        Assert.AreEqual("Werkgever 2", transaction2.NameOtherParty);
        Assert.IsNotNull(transaction2.AuthorizationCode);
        Assert.AreEqual(0, transaction2.AuthorizationCode.Length);
        Assert.AreEqual("Salaris 2", transaction2.Description);
    }

    [TestMethod]
    public async Task HandleAsync_SkipExistingTransaction_InsertNewTransaction_Success()
    {
        // Arrange
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(
            "\"IBAN/BBAN\",\"Munt\",\"BIC\",\"Volgnr\",\"Datum\",\"Rentedatum\",\"Bedrag\",\"Saldo na trn\",\"Tegenrekening IBAN/BBAN\",\"Naam tegenpartij\",\"Naam uiteindelijke partij\",\"Naam initi\ufffdrende partij\",\"BIC tegenpartij\",\"Code\",\"Batch ID\",\"Transactiereferentie\",Machtigingskenmerk,\"Incassant ID\",\"Betalingskenmerk\",\"Omschrijving-1\",\"Omschrijving-2\",\"Omschrijving-3\",\"Reden retour\",\"Oorspr bedrag\",\"Oorspr munt\",\"Koers\"");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012107\",\"2023-11-20\",\"2023-11-20\",\"+4000,00\",\"+4000,00\",\"NL11INGB00022222\",\"Werkgever 1\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423687\",,,,\"Salaris 1\",\" \",,,,,\n");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012108\",\"2023-11-21\",\"2023-11-21\",\"-2000,00\",\"+2000,00\",\"NL11INGB00033333\",\"Werkgever 2\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423688\",,,,\"Salaris 2\",\" \",,,,,\n");
        await writer.FlushAsync();
        stream.Position = 0;

        var username = CreateUniqueUserName("user1");
        await using var db = await CreateContext(username);

        await db.Transactions.AddAsync(new Transaction
            {
                DateTransaction = new DateOnly(2023, 11, 20), FollowNumber = 12107, Iban = "NL11RABO0104946666",
                Currency = "EUR", User = username
            },
            TestContext.CancellationTokenSource.Token);
        await db.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
        db.ChangeTracker.Clear();

        var useCase = new TransactionsFileEtlUseCase(new TransactionRepository(db),
            NullLogger<TransactionsFileEtlUseCase>.Instance);

        // Act
        var result = await useCase.HandleAsync(stream, username);

        // Assert
        db.ChangeTracker.Clear();
        var transactions = await db.Transactions.ToListAsync(TestContext.CancellationTokenSource.Token);
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, transactions);
    }

    public TestContext TestContext { get; set; }
}