using NotificationService.Models;

namespace NotificationService.Services;

public class NotificationServiceImpl : INotificationService
{
    private static readonly List<Notification> _notifications = new();
    private static readonly object _lock = new();

    private readonly Dictionary<string, (string Title, string Body, NotificationType Type, string DefaultChannel)> _templateLibrary =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["feedback-approved"] = ("Yorumun onaylandı", "{{eventName}} etkinliği için yaptığın yorum yayında.", NotificationType.FeedbackApproved, "email"),
            ["feedback-rejected"] = ("Yorumun reddedildi", "Yorumunun reddedilme nedeni: {{reason}}", NotificationType.FeedbackRejected, "email"),
            ["feedback-replied"] = ("Organizatör yanıtı", "{{organizer}} yorumu yanıtladı: {{reply}}", NotificationType.FeedbackReplied, "email"),
            ["favorite-reminder"] = ("Favori etkinlik başlıyor", "{{eventName}} {{startTime}} tarihinde başlıyor. Hazır mısın?", NotificationType.FavoriteReminder, "sms"),
            ["calendar-reminder"] = ("Takvim hatırlatma", "{{eventName}} için hatırlatma: {{startTime}}", NotificationType.CalendarReminder, "email"),
            ["seat-lock-expired"] = ("Koltuk kilidin sona erdi", "{{seatCode}} koltuğu için süre doldu.", NotificationType.SeatLockExpired, "push"),
            ["ticket-cancel-received"] = ("İptal talebi alındı", "Biletin iptal talebi işlendi. Talep No: {{requestId}}", NotificationType.TicketCancellation, "email"),
            ["ticket-refund-approved"] = ("İade onaylandı", "{{amount}}{{currency}} tutarındaki iade tamamlandı.", NotificationType.TicketRefundApproved, "email"),
            ["ticket-refund-declined"] = ("İade reddedildi", "İade talebin reddedildi. Detay: {{reason}}", NotificationType.TicketRefundDeclined, "email"),
            ["promo-confirmation"] = ("Kupon uygulandı", "{{code}} kodu ile {{discount}}{{currency}} indirim sağlandı.", NotificationType.PromoConfirmation, "email"),
            ["ticket-checkin-confirmed"] = ("Giriş başarıyla yapıldı", "{{eventName}} etkinliğine giriş kayıt edildi.", NotificationType.QrCheckIn, "email")
        };

    public Task SendNotificationAsync(string userId, string title, string message, NotificationType type)
    {
        var request = new NotificationDispatchRequest
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Channel = "email"
        };

        return SendNotificationAsync(request);
    }

    public Task SendNotificationAsync(NotificationDispatchRequest request)
    {
        var (title, body) = ResolveTemplate(request);
        var notification = new Notification
        {
            UserId = request.UserId,
            Title = title,
            Message = body,
            Type = request.Type,
            Channel = string.IsNullOrWhiteSpace(request.Channel) ? ResolveDefaultChannel(request.TemplateKey) : request.Channel,
            TemplateKey = request.TemplateKey,
            Metadata = new Dictionary<string, string>(request.Data, StringComparer.OrdinalIgnoreCase)
        };

        lock (_lock)
        {
            _notifications.Add(notification);
        }

        Console.WriteLine($"[NOTIFICATION:{notification.Channel}] {notification.Type} - {notification.UserId}: {notification.Title}");
        return Task.CompletedTask;
    }

    public Task<List<Notification>> GetUserNotificationsAsync(string userId)
    {
        lock (_lock)
        {
            return Task.FromResult(_notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList());
        }
    }

    public Task MarkAsReadAsync(string notificationId)
    {
        lock (_lock)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
            }
        }

        return Task.CompletedTask;
    }

    private (string Title, string Body) ResolveTemplate(NotificationDispatchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.TemplateKey) && _templateLibrary.TryGetValue(request.TemplateKey, out var template))
        {
            var title = request.Title ?? Interpolate(template.Title, request.Data);
            var body = request.Message ?? Interpolate(template.Body, request.Data);
            request.Type = request.Type == NotificationType.EventReminder ? template.Type : request.Type;
            return (title, body);
        }

        var fallbackTitle = request.Title ?? "Evently bildirimi";
        var fallbackBody = request.Message ?? "Yeni bildiriminiz var.";
        return (fallbackTitle, fallbackBody);
    }

    private string ResolveDefaultChannel(string templateKey)
    {
        return _templateLibrary.TryGetValue(templateKey ?? string.Empty, out var template)
            ? template.DefaultChannel
            : "email";
    }

    private static string Interpolate(string template, Dictionary<string, string> data)
    {
        var result = template;
        foreach (var entry in data)
        {
            result = result.Replace($"{{{{{entry.Key}}}}}", entry.Value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
