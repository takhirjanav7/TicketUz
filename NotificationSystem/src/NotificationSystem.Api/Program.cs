using NotificationSystem.Api.Extensions;

namespace NotificationService // ← bu namespace boʻlsin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // appsettings.json ni oʻqiydi
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Bizning barcha servislarimizni roʻyxatga oladi
            builder.Services.AddNotificationServices(builder.Configuration);

            var host = builder.Build();

            Console.WriteLine("NotificationService ishga tushdi... Xabarlar kutilmoqda!");

            await host.RunAsync(); // abadiy ishlaydi
        }
    }
}