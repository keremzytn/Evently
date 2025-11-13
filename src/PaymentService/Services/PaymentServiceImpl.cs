using PaymentService.DTOs;
using PaymentService.Models;

namespace PaymentService.Services;

public class PaymentServiceImpl : IPaymentService
{
    public async Task<Payment> ProcessPaymentAsync(ProcessPaymentDto dto, string userId)
    {
        // Ödeme simülasyonu - gerçek bir ödeme gateway'i yerine
        await Task.Delay(1000); // Ödeme işlemi simülasyonu

        var payment = new Payment
        {
            TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
            UserId = userId,
            EventId = dto.EventId,
            Amount = dto.Amount,
            Status = SimulatePaymentResult(),
            CreatedAt = DateTime.UtcNow
        };

        return payment;
    }

    private PaymentStatus SimulatePaymentResult()
    {
        // %95 başarı oranı ile ödeme simülasyonu
        var random = new Random();
        return random.Next(100) < 95 ? PaymentStatus.Success : PaymentStatus.Failed;
    }
}

