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
}

