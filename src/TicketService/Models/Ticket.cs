namespace TicketService.Models;

public class Ticket
{
    public int Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string TicketCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Active;
    public byte[]? QRCodeImage { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }
    public string? SeatCode { get; set; }
    public string? SeatLockToken { get; set; }
    public TicketCancellationStatus CancellationStatus { get; set; } = TicketCancellationStatus.None;
    public DateTime? CancellationRequestedAt { get; set; }
    public RefundStatus RefundStatus { get; set; } = RefundStatus.NotStarted;
    public DateTime? RefundProcessedAt { get; set; }
    public string? RefundReference { get; set; }
}

public enum TicketStatus
{
    Active,
    Used,
    Cancelled
}

public enum TicketCancellationStatus
{
    None,
    Pending,
    Approved,
    Declined
}

public enum RefundStatus
{
    NotStarted,
    Pending,
    Completed,
    Failed
}

