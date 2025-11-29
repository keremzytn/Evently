namespace Shared.Events;

public class PromoUsageEvent
{
    public string PromoCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}
