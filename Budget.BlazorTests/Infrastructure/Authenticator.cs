using System.Diagnostics;
using OtpNet;
using Serilog;

namespace Budget.BlazorTests.Infrastructure;

internal static class Authenticator
{
    internal static async Task<bool> GuardAgainstUnauthenticated(this IPage page, IBrowser browser,
        IBrowserContext context, Uri rootUri, string username, string password, string otpSecret)
    {
        var stateFile = "state.json";

        string title = await page.TitleAsync();

        if (title == "Sign in to your account")
        {
            try
            {
                await page.GetByText("Use another account").ClickAsync(new LocatorClickOptions
                {
                    Timeout = 1000
                });
            }
            catch (Exception)
            {
                Debug.WriteLine("Skipping already signed in but troubles");
            }

            try
            {
                await page.GetByPlaceholder("Email, phone, or Skype").FillAsync(username, new LocatorFillOptions { Timeout = 2000});
                await page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
            }
            catch (Exception)
            {
                Debug.WriteLine("Filling in username failed. skipping...");
            }

            await page.GetByPlaceholder("Password").FillAsync(password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

            try
            {
                await page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
                await page.GetByRole(AriaRole.Button, new() { Name = "Accept" }).ClickAsync();
            }
            catch (Exception)
            {
                Debug.WriteLine("Skipping verification");
            }

            try
            {
                await page.GetByRole(AriaRole.Button, new() { Name = "No" }).ClickAsync();
            }
            catch (Exception)
            {
                Debug.WriteLine("Skipping remember me");
            }

            await context.StorageStateAsync(new() { Path = stateFile });

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return true;
        }

        return false;
    }
}