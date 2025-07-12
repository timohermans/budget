# Budget ApiClient

This client is generated using [Refitter](https://github.com/christianhelle/refitter).

## Usage

First install refitter using the following command:

```bash
dotnet tool install -g Refitter
```

Then run the following command to generate the client again (make sure you're in the `src/Budget.ApiClient` directory):

```bash
refitter -s .
```

To register the client in your application, you can use the following code:

```csharp
services.AddApiClientRegistration<BudgetApiOptions>(config, "BudgetApi")
    .AddRefitClient<IBudgetClient>()
    .WithBlazorTokenProvider();
```

Make sure to reference the `Hertmans.Shared.Auth` project.