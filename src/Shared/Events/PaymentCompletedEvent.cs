namespace Shared.Events;

public class PaymentCompletedEvent
{
    public int PaymentId { get; set; }
    public int TicketId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Success";
    public DateTime CompletedAt { get; set; }
}

