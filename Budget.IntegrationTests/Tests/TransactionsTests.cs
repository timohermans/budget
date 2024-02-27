using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using Budget.Core.Models;
using Budget.IntegrationTests.Config;
using Budget.IntegrationTests.Helpers;
using Budget.Pages.Pages.Transactions;
using FakeItEasy;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
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
        var client = await fixture.CreateAuthenticatedAppClientAsync(output, timeProvider);

        // act upload form
        var responseForm = await client.GetAsync("/transactions/upload");

        // assert upload form
        var document = await fixture.OpenHtmlOfAsync(responseForm.Content);
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

        var responseDashboard = await client.GetAsync(responseUpload.Headers.Location);

        var dashboardDoc = await fixture.OpenHtmlOfAsync(responseDashboard.Content);

        output.WriteLine(dashboardDoc.Body?.InnerHtml);

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
            dashboardDoc.All.First(el => el.TextContent == "Inkomen").Closest("li")?.TextContent.Should()
                .Contain("6000", "I need to see the income from last month");
            dashboardDoc.All.First(el => el.TextContent == "Lasten").Closest("li")?.TextContent.Should()
                .Contain("1800", "I need to see the fixed expenses from last month");
            dashboardDoc.All.First(el => el.TextContent == "Budget").Closest("li")?.TextContent.Should()
                .Contain("4200", "I need to see the budget for this month");
            dashboardDoc.All.First(el => el.TextContent == "Weken").Closest("li")?.TextContent.Should()
                .Contain("5", "I need to see how many weeks there are in December");
            dashboardDoc.All.First(el => el.TextContent == "Budget per week").Closest("li")?.TextContent.Should()
                .Contain("840", "I need to see the budget per week");
            dashboardDoc.All.First(el => el.TextContent == "Week 48 👈").Closest("li")?.TextContent.Should()
                .Contain("-90,75", "I need to see what I spent in week 48");
            dashboardDoc.All.First(el => el.TextContent == "Week 48 👈").Closest("li")?.TextContent.Should()
                .Contain("749,25", "I need to see what I have left in week 48");
            dashboardDoc.All.First(el => el.TextContent == "Week 49").Closest("li")?.TextContent.Should()
                .Contain("-80,75", "I need to see what I spent in week 49");
            dashboardDoc.All.First(el => el.TextContent == "Uitgegeven").Closest("li")?.TextContent.Should()
                .Contain("-171,50", "I need to see what I spent in total");
            dashboardDoc.All.First(el => el.TextContent == "Over").Closest("li")?.TextContent.Should()
                .Contain("4028,50", "I need to see what total budget I have left");

            dashboardDoc.QuerySelectorAll("tbody tr").Should().HaveCount(4)
                .And
                .SatisfyRespectively(
                    row => row.TextContent.Should().Contain("Week 48"),
                    row => row.TextContent.Should().Contain("-90,75").And.Contain("Albert Heijn").And
                        .Contain("NL12INGB00044444"),
                    row => row.TextContent.Should().Contain("Week 49"),
                    row => row.TextContent.Should().Contain("-80,75").And.Contain("Albert Heijn").And
                        .Contain("NL12INGB00044444")
                );
        }
    }

    [Fact]
    public async Task Marks_transaction_as_cashback()
    {
        // Arrange
        var cashbackUrl = "/transactions/markascashback";
        var timeProvider = A.Fake<TimeProvider>();
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(new DateTimeOffset(new DateTime(2024, 1, 1)));

        var transactionToCashback = new Transaction
        {
            Currency = "EUR",
            Iban = "NL22RABO0101010100",
            Description = "Factuur x001",
            DateTransaction = new DateOnly(2023, 12, 7),
            Amount = 173,
            FollowNumber = 1,
            BalanceAfterTransaction = 173,
            IbanOtherParty = "NL00TEGENBANK",
            NameOtherParty = "Cambrium B.V.",
        };

        await using (var db = await fixture.CreateTableClientAsync())
        {
            await db.Transactions.AddAsync(transactionToCashback);
            await db.SaveChangesAsync();
        }

        var client = await fixture.CreateAuthenticatedAppClientAsync(output, timeProvider, false);

        // GET and set
        var response = await client.GetAsync($"{cashbackUrl}?id={transactionToCashback.Id}");
        var document = await fixture.OpenHtmlOfAsync(response.Content);
        var form = document.QuerySelector<IHtmlFormElement>("form");
        form.Should().NotBeNull();
        form!.SetValues(new Dictionary<string, string> { { nameof(MarkAsCashbackModel.Date), "2023-12-07" } });
        form!.Attributes["hx-post"]!.Value.Should().Be(cashbackUrl);

        // POST and assert
        var finalResponse = await client.SendAsync(cashbackUrl, form);
        finalResponse.StatusCode.IsRedirected();
        finalResponse.Headers.Location.Should()
            .Be("/transactions?year=2023&month=12&iban=" + transactionToCashback.Iban.ToLower()
                , "it should stay on the same transactions page to ensure smooth results");

        await using (var db = await fixture.CreateTableClientAsync(false))
        {
            (await db.Transactions.SingleAsync(t => t.Id == transactionToCashback.Id))
                .CashbackForDate.Should().Be(new DateOnly(2023, 12, 7));
        }
    }
}