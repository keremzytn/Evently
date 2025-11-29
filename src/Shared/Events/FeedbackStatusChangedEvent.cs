namespace Shared.Events;

public class FeedbackStatusChangedEvent
{
    public string FeedbackId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ModeratorId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
