using PaymentSystem.Api.Services;

namespace PaymentSystem.Api.Configurations;

public static class DependicyInjectionConfigurations
{
    public static void ConfigureDI(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<INotificationPublisher, NotificationPublisher>();

        builder.Services.AddHttpClient(); 

        builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
        {
            client.BaseAddress = new Uri("http://authsystem:80");
        });
    }
}