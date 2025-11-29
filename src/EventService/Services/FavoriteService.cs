using EventService.Data;
using EventService.DTOs;
using EventService.Models;
using MongoDB.Driver;

namespace EventService.Services;

public class FavoriteService : IFavoriteService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(MongoDbContext context, ILogger<FavoriteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserFavorite> AddOrUpdateAsync(string userId, FavoriteRequestDto dto, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserFavorite>.Filter.Where(f => f.UserId == userId && f.EventId == dto.EventId);
        var existing = await _context.UserFavorites.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            var favorite = new UserFavorite
            {
                UserId = userId,
                EventId = dto.EventId,
                Labels = dto.Labels ?? new List<string>(),
                ReminderOffsetMinutes = dto.ReminderOffsetMinutes,
                Notifications = dto.Notifications,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.UserFavorites.InsertOneAsync(favorite, cancellationToken: cancellationToken);
            _logger.LogInformation("Favorite created for user {UserId} and event {EventId}", userId, dto.EventId);
            return favorite;
        }

        var update = Builders<UserFavorite>.Update
            .Set(f => f.Labels, dto.Labels ?? existing.Labels)
            .Set(f => f.ReminderOffsetMinutes, dto.ReminderOffsetMinutes ?? existing.ReminderOffsetMinutes)
            .Set(f => f.Notifications, dto.Notifications)
            .Set(f => f.UpdatedAt, DateTime.UtcNow);

        await _context.UserFavorites.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        existing.Labels = dto.Labels ?? existing.Labels;
        existing.ReminderOffsetMinutes = dto.ReminderOffsetMinutes ?? existing.ReminderOffsetMinutes;
        existing.Notifications = dto.Notifications;
        existing.UpdatedAt = DateTime.UtcNow;
        return existing;
    }

    public async Task<bool> RemoveAsync(string userId, string eventId, CancellationToken cancellationToken = default)
    {
        var result = await _context.UserFavorites.DeleteOneAsync(f => f.UserId == userId && f.EventId == eventId, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<PagedResult<UserFavorite>> ListAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var filter = Builders<UserFavorite>.Filter.Where(f => f.UserId == userId);
        var total = await _context.UserFavorites.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _context.UserFavorites
            .Find(filter)
            .SortByDescending(f => f.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<UserFavorite>(items, total, page, pageSize);
    }

    public async Task<UserFavorite?> UpdateSettingsAsync(string userId, string eventId, UpdateFavoriteRequestDto dto, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserFavorite>.Filter.Where(f => f.UserId == userId && f.EventId == eventId);
        var favorite = await _context.UserFavorites.Find(filter).FirstOrDefaultAsync(cancellationToken);
        if (favorite is null)
        {
            return null;
        }

        var update = Builders<UserFavorite>.Update
            .Set(f => f.UpdatedAt, DateTime.UtcNow);

        if (dto.Labels != null)
        {
            update = update.Set(f => f.Labels, dto.Labels);
            favorite.Labels = dto.Labels;
        }

        if (dto.ReminderOffsetMinutes.HasValue)
        {
            update = update.Set(f => f.ReminderOffsetMinutes, dto.ReminderOffsetMinutes);
            favorite.ReminderOffsetMinutes = dto.ReminderOffsetMinutes;
        }

        if (dto.Notifications.HasValue)
        {
            update = update.Set(f => f.Notifications, dto.Notifications.Value);
            favorite.Notifications = dto.Notifications.Value;
        }

        await _context.UserFavorites.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        favorite.UpdatedAt = DateTime.UtcNow;
        return favorite;
    }
}
