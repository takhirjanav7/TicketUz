using NotificationSystem.Api.Consumers;
using NotificationSystem.Api.Services;

namespace NotificationSystem.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Background worker (RabbitMQ tinglovchi)
        services.AddHostedService<NotificationConsumer>();

        // Email joʻnatuvchi servis
        services.AddSingleton<EmailService>(sp =>
        {
            var smtpSection = configuration.GetSection("Smtp");
            return new EmailService(
                smtpHost: smtpSection["Host"]!,
                smtpPort: int.Parse(smtpSection["Port"]!),
                username: smtpSection["Username"]!,
                password: smtpSection["Password"]!
            );
        });

        return services;
    }
}