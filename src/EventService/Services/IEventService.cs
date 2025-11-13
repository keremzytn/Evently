using EventService.DTOs;
using EventService.Models;

namespace EventService.Services;

public interface IEventService
{
    Task<List<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(string id);
    Task<Event> CreateEventAsync(CreateEventDto dto, string organizerId);
    Task<Event?> UpdateEventAsync(string id, UpdateEventDto dto, string organizerId);
    Task<bool> DeleteEventAsync(string id, string organizerId);
    Task<bool> UpdateTicketCountAsync(string id, int count);
}

