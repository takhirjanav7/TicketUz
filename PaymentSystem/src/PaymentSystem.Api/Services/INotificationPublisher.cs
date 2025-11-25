namespace PaymentSystem.Api.Services;

public interface INotificationPublisher
{
    Task PublishAsync(string routingKey, object data);
}