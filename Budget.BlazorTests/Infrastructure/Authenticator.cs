namespace Budget.BlazorTests.Infrastructure;

internal static class Authenticator
{
    internal static async Task<bool> GuardAgainstUnauthenticated(this IPage page, IBrowser browser, IBrowserContext context, Uri rootUri, string username, string password, string otpSecret)
    {
        var stateFile = "state.json";

        string title = await page.TitleAsync();

        if (title == "Sign in to your account")
        {
            await page.GetByPlaceholder("Email, phone, or Skype").FillAsync("testdummy@timohermansoutlook.onmicrosoft.com");
            await page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
            await page.GetByPlaceholder("Password").FillAsync("Zesty0-Reshoot-Accustom");
            await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

            // there will come a time for the 2fa to come back. so this code is commented out for now
            //var base32Bytes = Base32Encoding.ToBytes(otpSecret);
            //var otp = new Totp(base32Bytes);
            //var totp = otp.ComputeTotp();

            await context.StorageStateAsync(new() { Path = stateFile });

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            //await page.GetByRole(AriaRole.Navigation).InnerHTMLAsync(); // wait for login to complete, otherwise race conditions might occur

            return true;
        }

        return false;
    }
}
