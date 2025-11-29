namespace Shared.Events;

public class SeatStatusChangedEvent
{
    public string EventId { get; set; } = string.Empty;
    public string SeatCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? LockToken { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
