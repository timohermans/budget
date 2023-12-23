using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Budget.IntegrationTests;

[Collection("integration")]
public class BasicTestsTestFixture(TestFixture testFixture, ITestOutputHelper testOutputHelper) 
{
    [Fact]
    public async Task Home_redirects_to_transactions()
    {
        // Arrange
        var client = await testFixture.CreateAuthenticatedAppClientAsync(testOutputHelper);

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Transactions_overview_integration()
    {
        // Arrange
        var client = await testFixture.CreateAuthenticatedAppClientAsync(testOutputHelper);

        // Act
        var response = await client.GetAsync("/transactions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}