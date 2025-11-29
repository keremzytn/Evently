namespace TicketService.Models;

public class SeatLock
{
    public int Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string SeatCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string LockToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public SeatLockStatus Status { get; set; } = SeatLockStatus.Held;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum SeatLockStatus
{
    Held,
    Committed,
    Expired
}
