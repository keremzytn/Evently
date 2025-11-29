using EventService.DTOs;
using EventService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers;

[ApiController]
[Route("api/users/me/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var favorite = await _favoriteService.AddOrUpdateAsync(userId, dto, cancellationToken);
        return Ok(favorite);
    }

    [HttpGet]
    public async Task<IActionResult> ListFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = ResolveUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var favorites = await _favoriteService.ListAsync(userId, page, pageSize, cancellationToken);
        return Ok(favorites);
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> RemoveFavorite(string eventId, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var removed = await _favoriteService.RemoveAsync(userId, eventId, cancellationToken);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{eventId}")]
    public async Task<IActionResult> UpdateFavorite(string eventId, [FromBody] UpdateFavoriteRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunlu" });
        }

        var favorite = await _favoriteService.UpdateSettingsAsync(userId, eventId, dto, cancellationToken);
        if (favorite == null)
        {
            return NotFound();
        }

        return Ok(favorite);
    }

    private string? ResolveUserId()
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        return string.IsNullOrWhiteSpace(userId) ? null : userId;
    }
}
