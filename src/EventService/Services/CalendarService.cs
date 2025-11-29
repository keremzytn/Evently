using System.Globalization;
using System.Text;
using EventService.Data;
using EventService.DTOs;
using EventService.Models;
using MongoDB.Driver;

namespace EventService.Services;

public class CalendarService : ICalendarService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(MongoDbContext context, ILogger<CalendarService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CalendarEntry> CreateAsync(string userId, CalendarEntryRequestDto dto, CalendarEntrySource source, CancellationToken cancellationToken = default)
    {
        if (dto.EndUtc < dto.StartUtc)
        {
            throw new ArgumentException("EndUtc başlangıçtan önce olamaz");
        }

        var entry = new CalendarEntry
        {
            UserId = userId,
            EventId = dto.EventId,
            Title = dto.Title,
            StartUtc = dto.StartUtc,
            EndUtc = dto.EndUtc,
            ReminderMinutesBefore = dto.ReminderMinutesBefore,
            Source = source,
            SyncStatus = CalendarSyncStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.CalendarEntries.InsertOneAsync(entry, cancellationToken: cancellationToken);
        _logger.LogInformation("Calendar entry created for user {UserId} ({Title})", userId, dto.Title);
        return entry;
    }

    public async Task<bool> DeleteAsync(string userId, string entryId, CancellationToken cancellationToken = default)
    {
        var result = await _context.CalendarEntries.DeleteOneAsync(e => e.Id == entryId && e.UserId == userId, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<IReadOnlyList<CalendarEntry>> ListAsync(string userId, DateTime? rangeStart, DateTime? rangeEnd, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<CalendarEntry>.Filter;
        var filter = filterBuilder.Eq(e => e.UserId, userId);

        if (rangeStart.HasValue)
        {
            filter &= filterBuilder.Gte(e => e.StartUtc, rangeStart.Value);
        }

        if (rangeEnd.HasValue)
        {
            filter &= filterBuilder.Lte(e => e.EndUtc, rangeEnd.Value);
        }

        return await _context.CalendarEntries
            .Find(filter)
            .SortBy(e => e.StartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> ExportAsIcsAsync(string userId, DateTime? rangeStart, DateTime? rangeEnd, CancellationToken cancellationToken = default)
    {
        var entries = await ListAsync(userId, rangeStart, rangeEnd, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("BEGIN:VCALENDAR");
        builder.AppendLine("VERSION:2.0");
        builder.AppendLine("PRODID:-//Evently//Calendar Export//EN");

        foreach (var entry in entries)
        {
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"UID:{entry.Id}@evently");
            builder.AppendLine($"DTSTAMP:{FormatDate(DateTime.UtcNow)}");
            builder.AppendLine($"DTSTART:{FormatDate(entry.StartUtc)}");
            builder.AppendLine($"DTEND:{FormatDate(entry.EndUtc)}");
            builder.AppendLine($"SUMMARY:{entry.Title.Replace(",", "\\,")}");
            builder.AppendLine("END:VEVENT");
        }

        builder.AppendLine("END:VCALENDAR");
        return builder.ToString();
    }

    private static string FormatDate(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
    }
}
