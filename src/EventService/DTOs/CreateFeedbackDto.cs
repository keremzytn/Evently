using System.ComponentModel.DataAnnotations;

namespace EventService.DTOs;

public class CreateFeedbackDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
