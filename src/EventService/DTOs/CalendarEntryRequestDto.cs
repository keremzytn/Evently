using EventService.Models;
using System.ComponentModel.DataAnnotations;

namespace EventService.DTOs;

public class CalendarEntryRequestDto
{
    public string? EventId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartUtc { get; set; }

    [Required]
    public DateTime EndUtc { get; set; }

    public int ReminderMinutesBefore { get; set; } = 60;
}
