using EventService.DTOs;
using EventService.Models;
using EventService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers;

[ApiController]
[Route("api/users/me/calendar")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet]
    public async Task<IActionResult> ListEntries(
        [FromQuery] DateTime? rangeStart,
        [FromQuery] DateTime? rangeEnd,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var entries = await _calendarService.ListAsync(userId, rangeStart, rangeEnd, cancellationToken);
        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEntry([FromBody] CalendarEntryRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var entry = await _calendarService.CreateAsync(userId, dto, CalendarEntrySource.Manual, cancellationToken);
        return Ok(entry);
    }

    [HttpDelete("{entryId}")]
    public async Task<IActionResult> DeleteEntry(string entryId, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var removed = await _calendarService.DeleteAsync(userId, entryId, cancellationToken);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("export.ics")]
    public async Task<IActionResult> ExportAsIcs(
        [FromQuery] DateTime? rangeStart,
        [FromQuery] DateTime? rangeEnd,
        CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var ics = await _calendarService.ExportAsIcsAsync(userId, rangeStart, rangeEnd, cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", "evently-calendar.ics");
    }

    private string? ResolveUserId()
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        return string.IsNullOrWhiteSpace(userId) ? null : userId;
    }
}
