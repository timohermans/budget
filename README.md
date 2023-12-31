## Resources used

- [Azure Table Storage inspiration](https://www.troyhunt.com/working-with-154-million-records-on/)
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=docker-hub%2Ctable-storage)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
- [Azure Storage Table Design Guide](https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-design-guide)
- [Azure Storage Table types](https://learn.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model#property-types)
- [Transactional Batches examples for Azure Storage Table](https://github.com/Azure/azure-sdk-for-net/blob/Azure.Data.Tables_12.8.2/sdk/tables/Azure.Data.Tables/samples/Sample6TransactionalBatch.md)

## Getting started

### Azurite

Make sure you have Azurite, the emulator for Azure Storage, running. You can do this with Docker:

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 -v C:/Users/<user>/data/azurite:/data -d --name azurite mcr.microsoft.com/azure-storage/azurite
```

Note that if `C:/Users/<user>/data/azurite:/data` doesn't exist, you need to create the directory

### (Optional) Azure Storage Explorer

You can use Azure Storage Explorer to view the data in the emulator.
You can download it [here](https://azure.microsoft.com/en-us/features/storage-explorer/).
Or install it via winget:

```bash
winget install Microsoft.Azure.StorageExplorer
```

## Misc topics

### Running docker image

The docker image can be run as follows

```
docker run -d -p 8080:8080 --name budget `
 -e "ConnectionStrings__TransactionTable=DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://azurite:10002/devstoreaccount1" `
 -e "Admin__Username=timo" `
 -e "Admin__Password=yoga-reproduce-washtub" `
 -e "Admin__TwoFactorSecret=DXOYO6HKTWNXPBYEMGYWM2QEZ46GLC6P" `
 --network="budget" `
 budget
```

Note this run command is used to test locally. This will give inspiration as to which environment variables need to be set when running in production.

### Creating a new one time password

```csharp
        var key = KeyGeneration.GenerateRandomKey(20);

        var base32String = Base32Encoding.ToString(key);
        var base32Bytes = Base32Encoding.ToBytes(base32String);

        var otp = new Totp(base32Bytes);

        _logger.LogCritical(base32String);
```

### Making the app work on horizontal scaling architectures

So at one point I got an error and redirected immediately to the login page.
What happened is that I logged in on one instance that the container app was running.
The session took quite a long time, so I think the sessions got booted up on another instance that had a different anti-forgery key.
To solve this issue I had to add the following code to the `program.cs`:

```csharp
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection()
        .SetApplicationName("Budget")
        .PersistKeysToAzureBlobStorage(builder.Configuration.GetConnectionString("Storage"), "keys", "keys.xml");
}
```

Note that this is not 100% secure.
I should also be looking at encrypting the keys, though I haven't gotten around trying to fix that.
