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
}

public enum TicketStatus
{
    Active,
    Used,
    Cancelled
}

