using EventService.DTOs;
using EventService.Models;

namespace EventService.Services;

public interface IFeedbackService
{
    Task<EventFeedback> CreateAsync(string eventId, string userId, CreateFeedbackDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<EventFeedback>> ListByEventAsync(string eventId, FeedbackStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<EventFeedback?> GetAsync(string feedbackId, CancellationToken cancellationToken = default);
    Task<EventFeedback?> UpdateAsync(string feedbackId, string userId, UpdateFeedbackDto dto, CancellationToken cancellationToken = default);
    Task<EventFeedback?> UpdateStatusAsync(string feedbackId, string moderatorId, UpdateFeedbackStatusDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string feedbackId, string requestorId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<EventRatingSummary> GetSummaryAsync(string eventId, CancellationToken cancellationToken = default);
}
