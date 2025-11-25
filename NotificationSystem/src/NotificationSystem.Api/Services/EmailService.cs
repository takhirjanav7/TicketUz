using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.Json;

namespace NotificationSystem.Api.Services;

public class EmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;

    public EmailService(string smtpHost, int smtpPort, string username, string password)
    {
        _host = smtpHost;
        _port = smtpPort;
        _username = username;
        _password = password;
    }

    public async Task SendAsync(string to, string subject, string html)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("noreply@myapp.uz"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = html };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_username, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public (string Subject, string Html) GetTemplate(string type, JsonElement data)
    {
        return type switch
        {
            "auth.registered" => (
                "Xush kelibsiz!",
                $"<h2>Salom {data.GetProperty("name").GetString()}!</h2>" +
                "<p>Platformamizga muvaffaqiyatli ro'yxatdan o'tdingiz!</p>" +
                "<p>Emailingizni tasdiqlash uchun quyidagi linkni bosing:</p>" +
                "<p><a href='https://myapp.uz/confirm'>Tasdiqlash</a></p>"),

            "auth.login" => (
                "Kirish muvaffaqiyatli!",
                $"<h2>Salom {data.GetProperty("name").GetString()}!</h2>" +
                $"<p>Siz tizimga muvaffaqiyatli kirdingiz.</p>" +
                $"<p>Vaqti: {DateTime.UtcNow:AddHours(5):dd.MM.yyyy HH:mm} (Toshkent)</p>" +
                (data.TryGetProperty("provider", out _)
                    ? $"<p>Usul: <strong>{data.GetProperty("provider").GetString()}</strong></p>"
                    : "")),

            "booking.created" => (
                "Biletlaringiz tayyor!",
                $"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>" +
                $"<h1 style='color: #e50914;'>Tabriklaymiz, {data.GetProperty("name").GetString()}!</h1>" +
                $"<h2>Siz <b>{data.GetProperty("movie").GetString()}</b> filmiga bilet oldingiz!</h2>" +
                $"<p><strong>Kino:</strong> {data.GetProperty("cinema").GetString()} ({data.GetProperty("hall").GetString()})</p>" +
                $"<p><strong>Sana:</strong> {data.GetProperty("date").GetString()}</p>" +
                $"<p><strong>Vaqt:</strong> {data.GetProperty("time").GetString()}</p>" +
                $"<p><strong>Oʻrindiq:</strong> <span style='font-size: 18px; color: #e50914;'><b>{data.GetProperty("seat").GetString()}</b></span></p>" +
                $"<p><strong>Toʻlov:</strong> <b>{data.GetProperty("amount").GetInt64():N0} soʻm</b></p>" +
                $"<hr/><p><small>Bron ID: <code>{data.GetProperty("bookingId").GetInt64()}</code></small></p>" +
                $"<p style='color: #666;'>Kinoteatrda sizni kutamiz!</p>" +
                $"</div>"),

            "payment.success" => (
                "Toʻlov qabul qilindi!",
                $"<h2 style='color:green'>Rahmat!</h2>" +
                $"<p><b>{data.GetProperty("amount").GetInt64():N0} soʻm</b> toʻlovingiz muvaffaqiyatli amalga oshirildi.</p>" +
                $"<p>Tranzaksiya: <code>{data.GetProperty("transactionId").GetString()}</code></p>" +
                $"<p>Bron ID: #{data.GetProperty("bookingId").GetInt64()}</p>" +
                $"<p>Sana: {data.GetProperty("date").GetString()}</p>"),

            "auth.forgot-password" => (
                "Parolni tiklash",
                $"<h2>Parolni tiklash soʻrovi</h2>" +
                $"<p>Salom {data.GetProperty("email").GetString()}!</p>" +
                $"<p>Parolingizni tiklash uchun quyidagi linkni bosing (1 soat amal qiladi):</p>" +
                $"<p><a href='{data.GetProperty("resetLink").GetString()}'>Parolni tiklash</a></p>"),

            _ => ("Yangi xabar", "<p>Yangi notification keldi.</p>")
        };
    }
}