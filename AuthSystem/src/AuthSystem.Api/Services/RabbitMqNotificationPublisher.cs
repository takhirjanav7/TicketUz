using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthSystem.Api.Services;

public class RabbitMqNotificationPublisher : INotificationPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "notifications_exchange";

    public RabbitMqNotificationPublisher(IConnection connection)
    {
        _connection = connection;
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: "direct",
            durable: true,
            autoDelete: false);
    }

    // Bu sizning interfeysingizdagi yagona metod
    public Task PublishAsync(string routingKey, object data)
    {
        var message = JsonSerializer.Serialize(data);
        var body = Encoding.UTF8.GetBytes(message);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }
}