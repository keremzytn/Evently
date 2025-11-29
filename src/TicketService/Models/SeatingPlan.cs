namespace TicketService.Models;

public class SeatingPlan
{
    public int Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public string LayoutJson { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
