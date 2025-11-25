using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PaymentSystem.Api.Services;

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
        var message = new { type = routingKey, data, timestamp = DateTimeOffset.UtcNow };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;

        _channel.BasicPublish(Exchange, routingKey, props, body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}