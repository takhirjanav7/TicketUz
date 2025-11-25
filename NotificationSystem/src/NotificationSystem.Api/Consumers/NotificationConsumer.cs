using NotificationSystem.Api.Models;
using NotificationSystem.Api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationSystem.Api.Consumers;

public sealed class NotificationConsumer : BackgroundService
{
    private readonly EmailService _emailService;
    private IConnection? _connection;
    private IModel? _channel;

    private const string Exchange = "notifications";

    private const string Queue = "email_queue";

    public NotificationConsumer(EmailService emailService)
    {
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
        _channel.QueueDeclare(Queue, durable: true, exclusive: false, autoDelete: false);

        var events = new[] 
        { 
            "auth.registered", 
            "auth.login",
            "auth.forgot-password",
            "booking.created", 
            "payment.success" 
        };

        foreach (var ev in events)
            _channel.QueueBind(Queue, Exchange, ev);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<NotificationMessage>(json);
                if (message is null)
                    throw new InvalidOperationException("Failed to deserialize NotificationMessage.");

                JsonElement data;
                if (message.Data is JsonElement je)
                {
                    data = je;
                }
                else
                {
                    data = JsonSerializer.SerializeToElement(message.Data);
                }

                var email = data.GetProperty("email").GetString()!;
                var (subject, html) = _emailService.GetTemplate(message.Type, data);

                await _emailService.SendAsync(email, subject, html);

                if (_channel is not null && _channel.IsOpen)
                    _channel.BasicAck(ea.DeliveryTag, false);

                Console.WriteLine($"[EMAIL SENT] {message.Type} → {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[XATO] {ex.Message}");
                if (_channel is not null && _channel.IsOpen)
                {
                    try
                    {
                         _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                    catch (Exception nackEx)
                    {
                        Console.WriteLine($"[XATO] BasicNack failed: {nackEx.Message}");
                    }
                }
            }
        };

        _channel.BasicConsume(Queue, false, consumer);

        // Toʻxtatish signali kelguncha kutamiz
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}