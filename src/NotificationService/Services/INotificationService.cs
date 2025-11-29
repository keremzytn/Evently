using NotificationService.Models;

namespace NotificationService.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, NotificationType type);
    Task<List<Notification>> GetUserNotificationsAsync(string userId);
    Task MarkAsReadAsync(string notificationId);
}

