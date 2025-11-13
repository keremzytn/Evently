using EventService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly Services.IEventService _eventService;

    public EventsController(Services.IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(string id)
    {
        var eventItem = await _eventService.GetEventByIdAsync(id);

        if (eventItem == null)
            return NotFound(new { message = "Etkinlik bulunamadı" });

        return Ok(eventItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        // TODO: JWT'den organizerId alınacak
        var organizerId = Request.Headers["X-User-Id"].ToString();

        if (string.IsNullOrEmpty(organizerId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        var newEvent = await _eventService.CreateEventAsync(dto, organizerId);
        return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] UpdateEventDto dto)
    {
        var organizerId = Request.Headers["X-User-Id"].ToString();

        if (string.IsNullOrEmpty(organizerId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        var updatedEvent = await _eventService.UpdateEventAsync(id, dto, organizerId);

        if (updatedEvent == null)
            return NotFound(new { message = "Etkinlik bulunamadı veya yetkiniz yok" });

        return Ok(updatedEvent);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        var organizerId = Request.Headers["X-User-Id"].ToString();

        if (string.IsNullOrEmpty(organizerId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        var result = await _eventService.DeleteEventAsync(id, organizerId);

        if (!result)
            return NotFound(new { message = "Etkinlik bulunamadı veya yetkiniz yok" });

        return NoContent();
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", service = "Event Service", timestamp = DateTime.UtcNow });
    }
}

