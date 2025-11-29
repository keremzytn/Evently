using EventService.DTOs;
using EventService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers;

[ApiController]
[Route("api/events/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> SearchEvents([FromQuery] SearchRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters(CancellationToken cancellationToken)
    {
        var filters = await _searchService.GetFiltersAsync(cancellationToken);
        return Ok(filters);
    }
}
