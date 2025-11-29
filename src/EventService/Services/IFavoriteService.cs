using EventService.DTOs;
using EventService.Models;

namespace EventService.Services;

public interface IFavoriteService
{
    Task<UserFavorite> AddOrUpdateAsync(string userId, FavoriteRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string userId, string eventId, CancellationToken cancellationToken = default);
    Task<PagedResult<UserFavorite>> ListAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserFavorite?> UpdateSettingsAsync(string userId, string eventId, UpdateFavoriteRequestDto dto, CancellationToken cancellationToken = default);
}
