## Resources used

- [Azure Table Storage inspiration](https://www.troyhunt.com/working-with-154-million-records-on/)
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=docker-hub%2Ctable-storage)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
- [Azure Storage Table Design Guide](https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-design-guide)
- [Azure Storage Table types](https://learn.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model#property-types)
- [Transactional Batches examples for Azure Storage Table](https://github.com/Azure/azure-sdk-for-net/blob/Azure.Data.Tables_12.8.2/sdk/tables/Azure.Data.Tables/samples/Sample6TransactionalBatch.md)

## Getting started

First off, get a postgres database running locally with a `budget` user, password and database.

```shell
docker run --name postgres -e POSTGRES_PASSWORD=budget -e POSTGRES_USER=budget -e POSTGRES_DB=budget -p 5432:5432 -d postgres
```

Then run the migrations to get the database up to date.

```shell
cd Budget.Pages
dotnet ef database update
```

Then run the app.

```shell
dotnet run
```

## Development process

Right now, there are two versions of the app: one for Azure and one for bare metal.
The Azure version uses Azure Table Storage, but I will not develop any further on that version.
The development process for the bare metal version is as follows:

- Create a new feature branch
- Develop the feature
- Create a pull request to merge the feature branch into the `home` branch
  - From this point, the project will be unit- and integration tested automatically
- Merge the pull request
- Delete the feature branch
- When the tests succeed on the home branch, the following happens:
  - A docker image is built and tagged with `the commit hash` and `latest`
  - The docker image is pushed to my personal registry
  - The Github webhook triggers a deployment to my personal server automatically
- Before or after deployment, make sure new migrations are run on the production database
  - Run `dotnet ef migrations script <last-migration>` to get the SQL script for the new migrations
  - Ssh into the server (`ssh <docker-user>@<server-ip>`)
  - Run `docker exec -it database bash` to get a bash shell in the container (or `docker ps` first to see the running database)
  - Run `psql -U budget` to get a psql shell in the database
  - Run the SQL script to update the database



## Misc topics

### Running docker image

The `Budget.Pages` docker image can be run as follows

```csharp
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