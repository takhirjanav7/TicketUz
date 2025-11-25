using Microsoft.EntityFrameworkCore;
using PaymentSystem.Api.Configurations;
using PaymentSystem.Api.Persistense;

namespace PaymentSystem.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.ConfigureDB();
        builder.ConfigureDI();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (true || app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }


        // Disable HTTPS redirection in Docker
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Auto migrate DB
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

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
