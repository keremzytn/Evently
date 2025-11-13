using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly Services.IPaymentService _paymentService;

    public PaymentsController(Services.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(dto, userId);

            if (payment.Status == Models.PaymentStatus.Failed)
            {
                return BadRequest(new
                {
                    message = "Ödeme işlemi başarısız oldu",
                    transactionId = payment.TransactionId,
                    status = payment.Status.ToString()
                });
            }

            return Ok(new
            {
                message = "Ödeme başarıyla işlendi",
                transactionId = payment.TransactionId,
                amount = payment.Amount,
                status = payment.Status.ToString(),
                processedAt = payment.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", service = "Payment Service", timestamp = DateTime.UtcNow });
    }
}

