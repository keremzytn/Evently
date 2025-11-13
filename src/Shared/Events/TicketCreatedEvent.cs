namespace Shared.Events;

public class TicketCreatedEvent
{
    public int TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; }
}

