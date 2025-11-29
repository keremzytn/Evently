using EventService.Models;
using System.ComponentModel.DataAnnotations;

namespace EventService.DTOs;

public class UpdateFeedbackStatusDto
{
    [Required]
    public FeedbackStatus Status { get; set; }

    [MaxLength(500)]
    public string? ModeratorNote { get; set; }
}
