using AuthSystem.Api.Configurations;
using AuthSystem.Api.Persistense;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.ConfigureDB();
        builder.ConfigureJwt();
        builder.ConfigureDI();

        var app = builder.Build();

        // Swagger har doim ishlasin (Docker’da ham qulay bo‘ladi)
        app.UseSwagger();
        app.UseSwaggerUI();

        // Production’da HTTPS, Docker’da esa yo‘q
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.MapControllers();

        // === MUHIM QISM: SQL Server to‘liq tayyor bo‘lguncha kutamiz ===
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 5 marta urinish, har 3 sekundda
            const int maxRetryCount = 5;
            var retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                try
                {
                    retryCount++;
                    Console.WriteLine($"DB migration attempt {retryCount}/{maxRetryCount}...");

                    // Bu yerda Migrate() yoki EnsureCreated() — ikkalasi ham ishlaydi
                    dbContext.Database.Migrate();     // yoki dbContext.Database.EnsureCreated();

                    Console.WriteLine("Database migration successful!");
                    break;
                }
                catch (Exception ex) when (retryCount < maxRetryCount)
                {
                    Console.WriteLine($"DB not ready yet: {ex.Message}");
                    await Task.Delay(3000); // 3 sekund kutamiz
                }
            }

            if (retryCount >= maxRetryCount)
            {
                Console.WriteLine("FATAL: Could not connect to SQL Server after multiple attempts.");
                Environment.Exit(1); // Agar DB ulanmasa, umuman ishga tushirmaymiz
            }
        }

        app.Run();
    }
}