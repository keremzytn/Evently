using System.ComponentModel.DataAnnotations;
using PaymentService.Models;

namespace PaymentService.DTOs;

public class PromoValidationRequestDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal CartTotal { get; set; }

    public string? EventId { get; set; }
    public string? OrganizerId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class PromoValidationResponseDto
{
    public bool IsValid { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public Guid? PromoId { get; set; }
    public PromoType? Type { get; set; }
}

public class PromoRedeemRequestDto : PromoValidationRequestDto
{
    public string OrderId { get; set; } = string.Empty;
    public string Channel { get; set; } = "web";
}
