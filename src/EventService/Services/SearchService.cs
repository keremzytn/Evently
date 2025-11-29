using EventService.Data;
using EventService.DTOs;
using EventService.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventService.Services;

public class SearchService : ISearchService
{
    private readonly MongoDbContext _context;

    public SearchService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Event>> SearchAsync(SearchRequestDto request, CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<Event>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var regex = new BsonRegularExpression(request.Q, "i");
            filter &= filterBuilder.Or(
                filterBuilder.Regex(e => e.Title, regex),
                filterBuilder.Regex(e => e.Description, regex));
        }

        if (request.CategoryIds?.Count > 0)
        {
            filter &= filterBuilder.In(e => e.Category, request.CategoryIds);
        }

        if (request.StartDate.HasValue)
        {
            filter &= filterBuilder.Gte(e => e.StartDate, request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            filter &= filterBuilder.Lte(e => e.EndDate, request.EndDate.Value);
        }

        if (request.MinPrice.HasValue)
        {
            filter &= filterBuilder.Gte(e => e.Price, request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            filter &= filterBuilder.Lte(e => e.Price, request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            filter &= filterBuilder.Regex(e => e.Location, new BsonRegularExpression(request.City, "i"));
        }

        var query = _context.Events.Find(filter);

        query = request.Sort switch
        {
            "date" => query.SortBy(e => e.StartDate),
            "price_asc" => query.SortBy(e => e.Price),
            "price_desc" => query.SortByDescending(e => e.Price),
            _ => query.SortBy(e => e.Title)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = await query.CountDocumentsAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<Event>(items, total, page, pageSize);
    }

    public async Task<SearchFiltersResponse> GetFiltersAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Events.Distinct(e => e.Category, FilterDefinition<Event>.Empty).ToListAsync(cancellationToken);
        var cities = await _context.Events.Distinct(e => e.Location, FilterDefinition<Event>.Empty).ToListAsync(cancellationToken);

        var priceBands = new[] { "0-100", "100-250", "250-500", "500+" };
        var dateRanges = new[] { "today", "weekend", "next_30_days" };

        return new SearchFiltersResponse
        {
            Categories = categories,
            Cities = cities,
            PriceBands = priceBands,
            DateRanges = dateRanges
        };
    }
}
