using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly Services.INotificationService _notificationService;

    public NotificationsController(Services.INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpPost("{notificationId}/mark-read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        await _notificationService.MarkAsReadAsync(notificationId);
        return Ok(new { message = "Bildirim okundu olarak işaretlendi" });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        await _notificationService.SendNotificationAsync(
            request.UserId,
            request.Title,
            request.Message,
            request.Type
        );

        return Ok(new { message = "Bildirim gönderildi" });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", service = "Notification Service", timestamp = DateTime.UtcNow });
    }
}

public class SendNotificationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
}

