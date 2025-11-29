namespace EventService.DTOs;

public class SearchRequestDto
{
    public string? Q { get; set; }
    public List<string>? CategoryIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? City { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public double? RadiusKm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Sort { get; set; }
}

public class SearchFiltersResponse
{
    public IReadOnlyCollection<string> Categories { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Cities { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> PriceBands { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> DateRanges { get; init; } = Array.Empty<string>();
}
