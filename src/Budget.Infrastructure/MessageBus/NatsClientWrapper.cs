using System.Text.Json;
using Budget.Domain.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NATS.Net;

namespace Budget.Infrastructure.MessageBus;

public class NatsClientWrapper(IConfiguration config, ILogger<NatsClientWrapper> logger)
    : IMessageBusClient, IAsyncDisposable
{
    private readonly NatsClient
        _client = new(config.GetConnectionString("nats") ?? throw new NotImplementedException());

    public async Task PublishAsync<T>(string subject, T data)
    {
        string? json = null;
        var t = typeof(T);
        if (t.IsPrimitive || t == typeof(Guid))
        {
            json = data?.ToString();
        }
        else if (t.IsClass)
        {
            json = JsonSerializer.Serialize(data);
        }
        else
        {
            throw new NotImplementedException($"Type {t.Name} not supported for publishing");
        }

        logger.LogInformation("Publishing message to subject {Subject}: {Data}", subject, json);
        await _client.PublishAsync(subject, data);
    }

    public async IAsyncEnumerable<T?> SubscribeAsync<T>(string subject, string queueGroup)
    {
        await foreach (var message in _client.SubscribeAsync<string>(subject, queueGroup))
        {
            logger.LogInformation("Received message from subject {Subject}: {Data}", subject, message.Data);
            message.EnsureSuccess();
            var data = message.Data;
            if (data == null)
            {
                yield return default;
                continue;
            }

            var t = typeof(T);
            if (t.IsPrimitive)
            {
                yield return (T)Convert.ChangeType(data, t);
            }
            else if (t == typeof(Guid))
            {
                yield return (T)(object)Guid.Parse(data);
            }
            else if (t.IsClass)
            {
                yield return JsonSerializer.Deserialize<T>(data);
            }
            else
            {
                throw new NotImplementedException($"Unable to deserialize type {t.Name}");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}