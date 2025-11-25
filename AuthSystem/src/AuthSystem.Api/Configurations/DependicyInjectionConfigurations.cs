using AuthSystem.Api.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AuthSystem.Api.Configurations;

public static class DependicyInjectionConfigurations
{
    public static void ConfigureDI(this WebApplicationBuilder builder)
    {
        // Avvalgilaringiz qoladi
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<INotificationPublisher, NotificationPublisher>();
        builder.Services.AddScoped<INotificationPublisher, RabbitMqNotificationPublisher>();

        // 1. RabbitMQ konfiguratsiyasini o‘qish
        builder.Services.Configure<RabbitMqConfiguration>(
            builder.Configuration.GetSection("RabbitMq"));

        // 2. Doimiy RabbitMQ ulanishi (Singleton — bir marta yaratiladi)
        builder.Services.AddSingleton<IConnection>(static sp =>
        {
            var rabbitConfig = sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;

            var factory = new ConnectionFactory()
            {
                HostName = rabbitConfig.HostName,   
                Port = rabbitConfig.Port,
                UserName = rabbitConfig.UserName,
                Password = rabbitConfig.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true    
            };

            return factory.CreateConnection("AuthSystem-Connection");
        });

        // 3. NotificationPublisher ni ro‘yxatdan o‘tkazish
        builder.Services.AddScoped<INotificationPublisher, RabbitMqNotificationPublisher>();
    }
}