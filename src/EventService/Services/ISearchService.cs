using EventService.DTOs;
using EventService.Models;

namespace EventService.Services;

public interface ISearchService
{
    Task<PagedResult<Event>> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default);
    Task<SearchFiltersResponse> GetFiltersAsync(CancellationToken cancellationToken = default);
}
