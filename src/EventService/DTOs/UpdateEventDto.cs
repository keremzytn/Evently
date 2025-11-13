namespace EventService.DTOs;

public class UpdateEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
}

