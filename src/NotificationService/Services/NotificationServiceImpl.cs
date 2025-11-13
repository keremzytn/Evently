using NotificationService.Models;

namespace NotificationService.Services;

public class NotificationServiceImpl : INotificationService
{
    // In-memory storage - gerçek uygulamada bir veritabanı kullanılır
    private static readonly List<Notification> _notifications = new();

    public Task SendNotificationAsync(string userId, string title, string message, NotificationType type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        _notifications.Add(notification);

        // Gerçek uygulamada burada email veya push notification gönderilir
        Console.WriteLine($"[NOTIFICATION] {type} - {userId}: {title}");

        return Task.CompletedTask;
    }

    public Task<List<Notification>> GetUserNotificationsAsync(string userId)
    {
        var userNotifications = _notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return Task.FromResult(userNotifications);
    }

    public Task MarkAsReadAsync(string notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);

        if (notification != null)
        {
            notification.IsRead = true;
        }

        return Task.CompletedTask;
    }
}

