namespace PaymentService.Models;

public enum PromoType
{
    Percentage,
    Fixed,
    Bogo
}

public enum PromoStatus
{
    Draft,
    Active,
    Inactive,
    Expired
}

public enum PromoScope
{
    Global,
    Event,
    Organizer
}

public class PromoCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public PromoType Type { get; set; } = PromoType.Percentage;
    public decimal Value { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? UsageLimit { get; set; }
    public int? PerUserLimit { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public PromoScope AppliesTo { get; set; } = PromoScope.Global;
    public string? TargetId { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public PromoStatus Status { get; set; } = PromoStatus.Active;
    public string CreatedBy { get; set; } = "system";
}

public class PromoUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PromoCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    public decimal DiscountAmount { get; set; }
    public string Channel { get; set; } = "web";
}

public class PromoValidationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PromoCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal CartTotal { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ClientIp { get; set; } = string.Empty;
}
