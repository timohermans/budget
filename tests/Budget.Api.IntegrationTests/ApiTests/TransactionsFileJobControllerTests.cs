using System.Net.Http.Json;
using Budget.Api.Models;
using Budget.Domain.Entities;

namespace Budget.Api.IntegrationTests.ApiTests;

[TestClass]
public class TransactionsFileJobControllerTests(TestContext testContext) : BaseApiTests(testContext)
{
    [TestMethod]
    public async Task GetById_ReturnsOk_WhenJobExists()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        await using var app = await CreateSut(
            nameof(GetById_ReturnsOk_WhenJobExists),
            CancellationToken.None);
    var (client, db) = app;

        var job = new TransactionsFileJob
        {
            Id = jobId,
            FileContent = [1, 2, 3, 4],
            OriginalFileName = "TestFile.csv",
            CreatedAt = DateTime.UtcNow,
            Status = Domain.Enums.JobStatus.Pending,
            User = "testuser"
        };
        db.TransactionsFileJobs.Add(job);
    await db.SaveChangesAsync(CancellationToken.None);

        // Act
    var response = await client.GetAsync($"/TransactionsFileJob/{jobId}", CancellationToken.None);

        // Assert
        response.EnsureSuccessStatusCode();
    var responseModel = await response.Content.ReadFromJsonAsync<TransactionsFileJobResponseModel>(cancellationToken: CancellationToken.None);
        Assert.IsNotNull(responseModel);
        Assert.AreEqual(job.Id, responseModel.Id);
        Assert.AreEqual(job.OriginalFileName, responseModel.OriginalFileName);
        Assert.AreEqual("Pending", responseModel.Status);
    }

    [TestMethod]
    public async Task GetById_ReturnsNotFound_WhenJobDoesNotExist()
    {
        // Arrange
        await using var app = await CreateSut(
            nameof(GetById_ReturnsNotFound_WhenJobDoesNotExist),
            CancellationToken.None);
    var (client, _) = app;

        // Act
    var response = await client.GetAsync($"/TransactionsFileJob/{Guid.NewGuid()}", CancellationToken.None);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
