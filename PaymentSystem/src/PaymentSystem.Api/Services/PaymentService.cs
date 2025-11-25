using Microsoft.EntityFrameworkCore;
using PaymentSystem.Api.Dtos;
using PaymentSystem.Api.Entities;
using PaymentSystem.Api.Persistense;

namespace PaymentSystem.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();
    private readonly INotificationPublisher _notifier;
    private readonly IAuthApiService _authApiService;

    public PaymentService(AppDbContext context, INotificationPublisher notifier, IAuthApiService authApiService)
    {
        _context = context;
        _notifier = notifier;
        _authApiService = authApiService;
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(PaymentCreateDto dto)
    {
        bool isSuccess = _random.Next(1, 101) <= 70;

        var payment = new Payment
        {
            UserId = dto.UserId,
            BookingId = dto.BookingId,
            Amount = dto.Amount,
            Status = isSuccess ? PaymentStatus.Success : PaymentStatus.Failed,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();

        if (isSuccess)
        {
            try
            {
                var user = await _authApiService.GetUserByIdAsync(dto.UserId);
                var userEmail = user?.Email ?? "no-email@example.com";
                var userName = user?.FirstName ?? userEmail.Split('@')[0];

                await _notifier.PublishAsync("payment.success", new
                {
                    email = userEmail,
                    name = userName,
                    amount = dto.Amount,
                    transactionId = $"TRX-{payment.PaymentId:D8}",
                    bookingId = dto.BookingId,
                    date = DateTime.UtcNow.AddHours(5).ToString("dd.MM.yyyy HH:mm", new System.Globalization.CultureInfo("uz-Latn-UZ"))
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTIFICATION XATOLIK] Payment {payment.PaymentId}: {ex.Message}");
            }
        }

        return new PaymentResultDto
        {
            UserId = payment.UserId,
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            Status = payment.Status,
            ProcessedAt = payment.CreatedAt
        };
    }

    public async Task<PaymentResultDto> GetPaymentAsync(long paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);

        if (payment == null)
            throw new Exception("Payment not found.");

        return new PaymentResultDto
        {
            UserId = payment.UserId,
            PaymentId = payment.PaymentId,
            BookingId = payment.BookingId,
            Status = payment.Status,
            ProcessedAt = payment.CreatedAt
        };
    }

    public async Task<List<PaymentResultDto>> GetAllPaymentsAsync()
    {
        return await _context.Payments
            .Select(payment => new PaymentResultDto
            {
                UserId = payment.UserId,
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Status = payment.Status,
                ProcessedAt = payment.CreatedAt
            })
            .ToListAsync();
    }
}
