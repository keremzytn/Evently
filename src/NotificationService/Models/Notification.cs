namespace NotificationService.Models;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Channel { get; set; } = "email";
    public string TemplateKey { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum NotificationType
{
    TicketPurchased,
    PaymentSuccess,
    PaymentFailed,
    EventReminder,
    EventCancelled,
    FeedbackApproved,
    FeedbackRejected,
    FeedbackReplied,
    FavoriteReminder,
    CalendarReminder,
    SeatLockExpired,
    TicketCancellation,
    TicketRefundApproved,
    TicketRefundDeclined,
    PromoConfirmation,
    QrCheckIn
}

