namespace Budget.Domain.Messaging;

public interface IMessageBusClient
{
    Task PublishAsync<T>(string subject, T data);
    IAsyncEnumerable<T?> SubscribeAsync<T>(string subject, string queueGroup);
}