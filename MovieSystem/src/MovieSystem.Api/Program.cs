
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MovieSystem.Api.Configurations;
using MovieSystem.Api.Persistense;

namespace MovieSystem.Api
{
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
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        Console.WriteLine($"DB migration attempt {i + 1}/10...");

                        db.Database.Migrate();  // Bu yerda xato chiqsa catch ga tushadi

                        Console.WriteLine("Database migration successful!");
                        break; // Muvaffaqiyatli bo‘lsa chiqamiz
                    }
                    catch (SqlException sqlEx) when (sqlEx.Number == 1801)
                    {
                        // 1801 = "Database 'XXX' already exists. Choose a different database name."
                        Console.WriteLine("Database already exists – this is normal, continuing...");
                        break; // Bu xato normal → davom etamiz
                    }
                    catch (Exception ex) when (i < 9)
                    {
                        Console.WriteLine($"DB not ready yet (attempt {i + 1}): {ex.Message}");
                        await Task.Delay(3000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"FATAL ERROR during migration: {ex}");
                        throw; // Boshqa jiddiy xato bo‘lsa → crash bo‘lsin
                    }
                }
            }


            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
