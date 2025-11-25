namespace BookingSystem.Api.Infrastructure;

public interface INotificationPublisher
{
    Task PublishAsync(string routingKey, object data);
}