using EventService.Models;
using System.ComponentModel.DataAnnotations;

namespace EventService.DTOs;

public class FavoriteRequestDto
{
    [Required]
    public string EventId { get; set; } = string.Empty;

    public List<string>? Labels { get; set; }

    [Range(5, 1440)]
    public int? ReminderOffsetMinutes { get; set; }

    public FavoriteNotificationChannel Notifications { get; set; } = FavoriteNotificationChannel.Email;
}

public class UpdateFavoriteRequestDto
{
    public List<string>? Labels { get; set; }

    [Range(5, 1440)]
    public int? ReminderOffsetMinutes { get; set; }

    public FavoriteNotificationChannel? Notifications { get; set; }
}
