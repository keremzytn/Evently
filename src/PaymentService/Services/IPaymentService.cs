using PaymentService.DTOs;
using PaymentService.Models;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<Payment> ProcessPaymentAsync(ProcessPaymentDto dto, string userId);
}

