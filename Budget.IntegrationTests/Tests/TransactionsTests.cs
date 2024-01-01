using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Budget.IntegrationTests.Helpers;
using FluentAssertions;
using FluentAssertions.Execution;

namespace Budget.IntegrationTests.Tests;

[Collection("integration")]
public class TransactionsTests(TestFixture fixture) {
    [Fact]
    public async Task Upload_transactions_from_a_file_and_show_nice_dashboard()
    {
        // arrange upload form 
        var client = await fixture.CreateAuthenticatedAppClientAsync();

        // act upload form
        var responseForm = await client.GetAsync("/transactions/upload");

        // assert upload form
        var document = await fixture.OpenHtmlOf(responseForm.Content);
        var form = document.QuerySelector<IHtmlFormElement>("form");
        using (new AssertionScope("Assert upload form"))
        {
            form.Should().NotBeNull().And.BeAssignableTo<IHtmlFormElement>();
            form?.GetAttribute("enctype").Should().BeEquivalentTo("multipart/form-data");
            form?.GetAttribute("method").Should().BeEquivalentTo("post");
            form?.QuerySelector("input[type='file'][name='TransactionsFile']").Should().NotBeNull();
            form?.QuerySelector("[type=submit]").Should().NotBeNull();
        }

        // arrange file upload
        var fileOptions = new FileUpload("Data/transactions-1.csv", "TransactionsFile", "transactions.csv");

        // act file upload
        var responseUpload = await client.SendAsync("/transactions/upload", form!, fileValues: fileOptions);

        // assert file upload
        responseUpload.Should().BeRedirection("because we should be redirected to the transaction overview page");
        responseUpload.Headers.Location.Should().Be("/transactions");

        // TODO: Make another call to transaction overview, as redirect doesn't show this (use Headers as the page to get!)

        var responseDashboard = await client.GetAsync(responseUpload.Headers.Location);
        
    }

}