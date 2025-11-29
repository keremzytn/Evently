using EventService.Models;
using MongoDB.Driver;

namespace EventService.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("MongoDB connection string bulunamadı");

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(configuration["MongoDB:DatabaseName"] ?? "evently_events");
    }

    public IMongoCollection<Event> Events => _database.GetCollection<Event>("events");
    public IMongoCollection<EventFeedback> EventFeedback => _database.GetCollection<EventFeedback>("event_feedback");
    public IMongoCollection<EventRatingSummary> EventFeedbackSummaries => _database.GetCollection<EventRatingSummary>("event_feedback_summary");
    public IMongoCollection<UserFavorite> UserFavorites => _database.GetCollection<UserFavorite>("user_favorites");
    public IMongoCollection<CalendarEntry> CalendarEntries => _database.GetCollection<CalendarEntry>("calendar_entries");
}

