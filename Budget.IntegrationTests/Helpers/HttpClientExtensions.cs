using AngleSharp.Html.Dom;

namespace Budget.IntegrationTests.Helpers;

public record FileUpload(string FilePath, string InputName, string FileName);

// pulled from https://github.com/dotnet/AspNetCore.Docs.Samples/blob/main/test/integration-tests/IntegrationTestsSample/tests/RazorPagesProject.Tests/Helpers/HttpClientExtensions.cs 
public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        IHtmlFormElement form,
        IHtmlElement submitButton)
    {
        return client.SendAsync(form, submitButton, new Dictionary<string, string>());
    }

    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        IHtmlFormElement form,
        IEnumerable<KeyValuePair<string, string>> formValues)
    {
        var submitElement = Assert.Single(form.QuerySelectorAll("[type=submit]"));
        var submitButton = Assert.IsAssignableFrom<IHtmlElement>(submitElement);

        return client.SendAsync(form, submitButton, formValues);
    }

    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        string requestUri,
        IHtmlFormElement form,
        FileUpload fileValues)
    {
        var submitElement = Assert.Single(form.QuerySelectorAll("[type=submit]"));
        var submitButton = Assert.IsAssignableFrom<IHtmlElement>(submitElement);

        var submission = CreateSubmissionFrom(form, submitButton);
        submission.RequestUri = new Uri(submission.RequestUri?.AbsoluteUri + (requestUri.StartsWith("/") ? string.Join("", requestUri.Skip(1)) : requestUri));

        submission.Content = new MultipartFormDataContent
        {
            { new StreamContent(File.OpenRead(fileValues.FilePath)), fileValues.InputName, fileValues.FileName }
        };

        return client.SendAsync(submission);
    }

    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        IHtmlFormElement form,
        IHtmlElement submitButton,
        IEnumerable<KeyValuePair<string, string>> formValues)
    {
        foreach (var kvp in formValues)
        {
            var element = Assert.IsAssignableFrom<IHtmlInputElement>(form[kvp.Key]);
            element.Value = kvp.Value;
        }

        var submission = CreateSubmissionFrom(form, submitButton);

        return client.SendAsync(submission);
    }

    private static HttpRequestMessage CreateSubmissionFrom(IHtmlFormElement form, IHtmlElement submitButton)
    {
        var submit = form.GetSubmission();
        Assert.NotNull(submit);
        var target = (Uri)submit.Target;
        if (submitButton.HasAttribute("formaction"))
        {
            var formaction = Assert.IsAssignableFrom<string>(submitButton.GetAttribute("formaction"));
            target = new Uri(formaction, UriKind.Relative);
        }
        var submission = new HttpRequestMessage(new HttpMethod(submit.Method.ToString()), target)
        {
            Content = new StreamContent(submit.Body)
        };

        foreach (var header in submit.Headers)
        {
            submission.Headers.TryAddWithoutValidation(header.Key, header.Value);
            submission.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return submission;
    }
}