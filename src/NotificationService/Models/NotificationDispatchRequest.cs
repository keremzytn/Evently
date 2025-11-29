namespace NotificationService.Models;

public class NotificationDispatchRequest
{
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.EventReminder;
    public string Channel { get; set; } = "email";
    public string TemplateKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
}
