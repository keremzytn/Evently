using EventService.Data;
using EventService.DTOs;
using EventService.Models;
using MongoDB.Driver;

namespace EventService.Services;

public class EventServiceImpl : IEventService
{
    private readonly MongoDbContext _context;

    public EventServiceImpl(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Event>> GetAllEventsAsync()
    {
        return await _context.Events.Find(_ => true).ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(string id)
    {
        return await _context.Events.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Event> CreateEventAsync(CreateEventDto dto, string organizerId)
    {
        var newEvent = new Event
        {
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalTickets = dto.TotalTickets,
            AvailableTickets = dto.TotalTickets,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            Category = dto.Category,
            OrganizerId = organizerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Events.InsertOneAsync(newEvent);
        return newEvent;
    }

    public async Task<Event?> UpdateEventAsync(string id, UpdateEventDto dto, string organizerId)
    {
        var existingEvent = await GetEventByIdAsync(id);

        if (existingEvent == null || existingEvent.OrganizerId != organizerId)
            return null;

        var updateBuilder = Builders<Event>.Update;
        var updates = new List<UpdateDefinition<Event>>();

        if (!string.IsNullOrEmpty(dto.Title))
            updates.Add(updateBuilder.Set(e => e.Title, dto.Title));

        if (!string.IsNullOrEmpty(dto.Description))
            updates.Add(updateBuilder.Set(e => e.Description, dto.Description));

        if (!string.IsNullOrEmpty(dto.Location))
            updates.Add(updateBuilder.Set(e => e.Location, dto.Location));

        if (dto.StartDate.HasValue)
            updates.Add(updateBuilder.Set(e => e.StartDate, dto.StartDate.Value));

        if (dto.EndDate.HasValue)
            updates.Add(updateBuilder.Set(e => e.EndDate, dto.EndDate.Value));

        if (dto.Price.HasValue)
            updates.Add(updateBuilder.Set(e => e.Price, dto.Price.Value));

        if (dto.ImageUrl != null)
            updates.Add(updateBuilder.Set(e => e.ImageUrl, dto.ImageUrl));

        if (!string.IsNullOrEmpty(dto.Category))
            updates.Add(updateBuilder.Set(e => e.Category, dto.Category));

        updates.Add(updateBuilder.Set(e => e.UpdatedAt, DateTime.UtcNow));

        if (updates.Count > 0)
        {
            var combinedUpdate = updateBuilder.Combine(updates);
            await _context.Events.UpdateOneAsync(e => e.Id == id, combinedUpdate);
        }

        return await GetEventByIdAsync(id);
    }

    public async Task<bool> DeleteEventAsync(string id, string organizerId)
    {
        var existingEvent = await GetEventByIdAsync(id);

        if (existingEvent == null || existingEvent.OrganizerId != organizerId)
            return false;

        var result = await _context.Events.DeleteOneAsync(e => e.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateTicketCountAsync(string id, int count)
    {
        var update = Builders<Event>.Update.Inc(e => e.AvailableTickets, -count);
        var result = await _context.Events.UpdateOneAsync(e => e.Id == id, update);
        return result.ModifiedCount > 0;
    }
}

