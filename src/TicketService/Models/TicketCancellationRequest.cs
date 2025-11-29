namespace TicketService.Models;

public enum CancellationDecision
{
    Pending,
    Approved,
    Declined
}

public class TicketCancellationRequest
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public CancellationDecision Decision { get; set; } = CancellationDecision.Pending;
    public decimal? RefundAmount { get; set; }
    public string? RefundCurrency { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
    public string? Notes { get; set; }
}
