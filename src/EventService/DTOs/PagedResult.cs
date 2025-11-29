namespace EventService.DTOs;

public record PagedResult<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize);
