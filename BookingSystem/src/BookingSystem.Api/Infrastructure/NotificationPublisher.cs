using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace BookingSystem.Api.Infrastructure;

public class NotificationPublisher : INotificationPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly RabbitMQ.Client.IModel _channel;
    private const string Exchange = "notifications";

    public NotificationPublisher()
    {
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ? "host.docker.internal"
            : "rabbitmq-1",

            Port = 5672,
            UserName = "guest",
            Password = "guest",
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Direct, durable: true);
    }

    public async Task PublishAsync(string routingKey, object data)
    {
        var message = new
        {
            type = routingKey,
            data,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        // await bor — xavfsiz va ishonchli
        _channel.BasicPublish(
            exchange: Exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}