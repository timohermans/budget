using System.Net;
using FluentAssertions;

namespace Budget.IntegrationTests.Transactions;

[Collection("integration")]
public class UploadTests(TestFixture fixture)
{

    [Fact]
    public async Task Displays_a_nice_form_to_upload_transactions()
    {
        var client = await fixture.CreateAuthenticatedAppClientAsync();

        var response = await client.GetAsync("/transactions/upload");

        var document = await fixture.OpenHtmlOf(await response.Content.ReadAsStringAsync());

        var form = document.QuerySelector("form");
        form.Should().NotBeNull();
        form?.GetAttribute("enctype").Should().BeEquivalentTo("multipart/form-data");
        form?.GetAttribute("method").Should().BeEquivalentTo("post");
        form?.QuerySelector("input[type='file'][name='TransactionsFile']").Should().NotBeNull();
        form?.QuerySelector("[type=submit]").Should().NotBeNull();


        client.SendAsync()
    }

    [Fact]
    public async Task Posts_transactions_file_and_processes_successfully()
    {
        var client = await fixture.CreateAuthenticatedAppClientAsync();

        using var file = File.OpenRead("Transactions/UploadTests1.csv");

        var response = await client.PostAsync("/transactions/upload", new MultipartFormDataContent
        {
            { new StreamContent(file), "TransactionsFile" , "transactions.csv" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().Be("/transactions");
    }

}