using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Budget.IntegrationTests.Config;
using Budget.IntegrationTests.Helpers;
using FakeItEasy;
using FluentAssertions.Execution;
using Xunit.Abstractions;

namespace Budget.IntegrationTests.Tests;

[Collection("integration")]
public class TransactionsTests(TestFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public async Task Upload_transactions_from_a_file_and_show_nice_dashboard()
    {
        // global arrange
        var timeProvider = A.Fake<TimeProvider>();
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(new DateTime(2023, 12, 1));

        // arrange upload form 
        output.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} - creating client");
        var client = await fixture.CreateAuthenticatedAppClientAsync(output, timeProvider);

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
        var fileOptions = new FileUpload("Data/transactions-2.csv", "TransactionsFile", "transactions.csv");

        // act file upload
        var responseUpload = await client.SendAsync("/transactions/upload", form!, fileValues: fileOptions);

        // assert file upload
        responseUpload.Should().BeRedirection("because we should be redirected to the transaction overview page");
        responseUpload.Headers.Location.Should().Be("/transactions");

        // TODO: Make another call to transaction overview, as redirect doesn't show this (use Headers as the page to get!)
        var responseDashboard = await client.GetAsync(responseUpload.Headers.Location);

        var dashboardDoc = await fixture.OpenHtmlOf(responseDashboard.Content);

        using (new AssertionScope("Dashboard data"))
        {
            dashboardDoc.QuerySelector("[href='/transactions?year=2023&month=11']").Should()
                .NotBeNull("I need to go back to November");
            dashboardDoc.QuerySelector("[href='/transactions?year=2024&month=1']").Should()
                .NotBeNull("I need to go forward to January");
            dashboardDoc.QuerySelector(".navbar-brand")?.TextContent.Should()
                .Contain("2023-12", "I need to see the current month");
            dashboardDoc.QuerySelector("[href='/transactions/upload']").Should()
                .NotBeNull("I need to be able to upload new transactions");
            dashboardDoc.All.First(el => el.TextContent == "Inkomen").ParentElement?.TextContent.Should()
                .Contain("6000", "I need to see the income from last month");
            dashboardDoc.All.First(el => el.TextContent == "Lasten").ParentElement?.TextContent.Should()
                .Contain("1800", "I need to see the fixed expenses from last month");
            dashboardDoc.All.First(el => el.TextContent == "Budget").ParentElement?.TextContent.Should()
                .Contain("4200", "I need to see the budget for this month");
            dashboardDoc.All.First(el => el.TextContent == "Weken").ParentElement?.TextContent.Should()
                .Contain("5", "I need to see how many weeks there are in December");
            dashboardDoc.All.First(el => el.TextContent == "Budget per week").ParentElement?.TextContent.Should()
                .Contain("840", "I need to see the budget per week");
            dashboardDoc.All.First(el => el.TextContent == "Week 48 👈").ParentElement?.TextContent.Should()
                .Contain("-90,75", "I need to see what I spent in week 48");
            dashboardDoc.All.First(el => el.TextContent == "Week 48 👈").ParentElement?.TextContent.Should()
                .Contain("749,25", "I need to see what I have left in week 48");
            // dashboardDoc.All.First(el => el.TextContent == "Uitgegeven").ParentElement?.TextContent.Should()
            //     .Contain("90,75", "I need to see what I spent in total"); // TODO: is nog niet geimplementeerd
            dashboardDoc.All.First(el => el.TextContent == "Over").ParentElement?.TextContent.Should()
                .Contain("4109,25", "I need to see what total budget I have left");

            dashboardDoc.QuerySelectorAll("tbody tr").Should().HaveCount(2)
                .And
                .SatisfyRespectively(
                    row => row.TextContent.Should().Contain("Week 48"),
                    row => row.TextContent.Should().Contain("-90,75").And.Contain("Albert Heijn").And
                        .Contain("NL12INGB00044444")
                );
        }

        output.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} - done");
    }
}