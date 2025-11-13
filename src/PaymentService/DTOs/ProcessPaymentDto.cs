using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs;

public class ProcessPaymentDto
{
    [Required(ErrorMessage = "Etkinlik ID gereklidir")]
    public string EventId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tutar gereklidir")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Kart numarası gereklidir")]
    [CreditCard(ErrorMessage = "Geçersiz kart numarası")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kart sahibi adı gereklidir")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Son kullanma tarihi gereklidir")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Geçersiz tarih formatı (MM/YY)")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV gereklidir")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV 3 haneli olmalıdır")]
    public string CVV { get; set; } = string.Empty;
}

