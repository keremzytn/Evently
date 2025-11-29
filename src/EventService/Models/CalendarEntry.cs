using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventService.Models;

public enum CalendarEntrySource
{
    Favorite,
    Ticket,
    Manual
}

public enum CalendarSyncStatus
{
    Pending,
    Synced,
    Failed
}

public class CalendarEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("eventId")]
    public string? EventId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("startUtc")]
    public DateTime StartUtc { get; set; }

    [BsonElement("endUtc")]
    public DateTime EndUtc { get; set; }

    [BsonElement("source")]
    public CalendarEntrySource Source { get; set; } = CalendarEntrySource.Manual;

    [BsonElement("reminderMinutesBefore")]
    public int ReminderMinutesBefore { get; set; } = 60;

    [BsonElement("syncStatus")]
    public CalendarSyncStatus SyncStatus { get; set; } = CalendarSyncStatus.Pending;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
