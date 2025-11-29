namespace Shared.Events;

public class TicketCancellationRequestedEvent
{
    public int TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
