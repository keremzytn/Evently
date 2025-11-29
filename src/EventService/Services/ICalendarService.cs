using EventService.DTOs;
using EventService.Models;

namespace EventService.Services;

public interface ICalendarService
{
    Task<CalendarEntry> CreateAsync(string userId, CalendarEntryRequestDto dto, CalendarEntrySource source, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string userId, string entryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalendarEntry>> ListAsync(string userId, DateTime? rangeStart, DateTime? rangeEnd, CancellationToken cancellationToken = default);
    Task<string> ExportAsIcsAsync(string userId, DateTime? rangeStart, DateTime? rangeEnd, CancellationToken cancellationToken = default);
}
