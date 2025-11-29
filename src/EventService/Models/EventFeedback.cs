using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventService.Models;

public enum FeedbackStatus
{
    Pending,
    Approved,
    Rejected
}

public class EventFeedback
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("rating")]
    public int Rating { get; set; }

    [BsonElement("comment")]
    public string Comment { get; set; } = string.Empty;

    [BsonElement("status")]
    public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("moderatorId")]
    public string? ModeratorId { get; set; }

    [BsonElement("moderatorNote")]
    public string? ModeratorNote { get; set; }
}

public class EventRatingSummary
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty;

    [BsonElement("averageRating")]
    public double AverageRating { get; set; }

    [BsonElement("reviewCount")]
    public long ReviewCount { get; set; }

    [BsonElement("lastCalculatedAt")]
    public DateTime LastCalculatedAt { get; set; } = DateTime.UtcNow;
}
