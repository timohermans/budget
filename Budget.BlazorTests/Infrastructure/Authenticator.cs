namespace Budget.BlazorTests.Infrastructure;

internal static class Authenticator
{
    internal static async Task<bool> GuardAgainstUnauthenticated(this IPage page, IBrowser browser, IBrowserContext context, Uri rootUri, string username, string password)
    {
        var stateFile = "state.json";

        string title = await page.TitleAsync();

        if (title == "Sign in to budget")
        {
            await page.GetByLabel("Username or email").ClickAsync();
            await page.GetByLabel("Username or email").FillAsync(username);
            await page.GetByLabel("Username or email").PressAsync("Tab");
            await page.GetByLabel("Password", new() { Exact = true }).FillAsync(password);
            await page.GetByText("Remember me").ClickAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

            await context.StorageStateAsync(new() { Path = stateFile });

            await page.GetByRole(AriaRole.Navigation).InnerHTMLAsync(); // wait for login to complete, otherwise race conditions might occur

            return true;
        }

        return false;
    }
}
