using Azure.Data.Tables;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Budget.Tests.Config;

public class DataFixture : IDisposable
{
    private readonly IContainer _container;

    public DataFixture()
    {
        _container = new ContainerBuilder()
          .WithImage("mcr.microsoft.com/azure-storage/azurite")
          .WithPortBinding(10002, true)
          .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Azurite Table service is successfully listening at http://0.0.0.0:10002"))
          .Build();
    }

    public async Task<TableClient> CreateTableClientAsync()
    {
        if (_container.State != TestcontainersStates.Running)
        {
            await _container.StartAsync().ConfigureAwait(false);
        }

        var service = new TableServiceClient($"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://{_container.Hostname}:{_container.GetMappedPublicPort(10002)}/devstoreaccount1");
        var client = service.GetTableClient("Transactions");
        await client.DeleteAsync();
        client.CreateIfNotExists();
        return client;
    }

    public void Dispose()
    {
        _container.DisposeAsync();
    }
}


[CollectionDefinition("data")]
public class DataCollectionDefinition : ICollectionFixture<DataFixture>
{
}