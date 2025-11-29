using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventService.Models;

[Flags]
public enum FavoriteNotificationChannel
{
    None = 0,
    Email = 1,
    Sms = 2,
    Push = 4
}

public class UserFavorite
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("labels")]
    public List<string> Labels { get; set; } = new();

    [BsonElement("reminderOffsetMinutes")]
    public int? ReminderOffsetMinutes { get; set; }

    [BsonElement("notifications")]
    public FavoriteNotificationChannel Notifications { get; set; } = FavoriteNotificationChannel.Email;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
