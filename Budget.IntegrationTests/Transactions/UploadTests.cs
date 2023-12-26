using System.Net;
using AngleSharp.Html.Dom;
using Budget.IntegrationTests.Helpers;
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

        var form = Assert.IsAssignableFrom<IHtmlFormElement>(document.QuerySelector("form"));
        Assert.NotNull(form);
        form.Should().NotBeNull();
        form?.GetAttribute("enctype").Should().BeEquivalentTo("multipart/form-data");
        form?.GetAttribute("method").Should().BeEquivalentTo("post");
        form?.QuerySelector("input[type='file'][name='TransactionsFile']").Should().NotBeNull();
        form?.QuerySelector("[type=submit]").Should().NotBeNull();

        var fileOptions = new FileUpload("Transactions/UploadTests1.csv", "TransactionsFile", "transactions.csv");
        var result = await client.SendAsync("/transactions/upload", form!, fileOptions);

        result.EnsureSuccessStatusCode();

        result.Headers.Location.Should().Be("/transactions");
    }


}