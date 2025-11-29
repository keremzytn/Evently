using EventService.Data;
using EventService.DTOs;
using EventService.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventService.Services;

public class FeedbackService : IFeedbackService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(MongoDbContext context, ILogger<FeedbackService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EventFeedback> CreateAsync(string eventId, string userId, CreateFeedbackDto dto, CancellationToken cancellationToken = default)
    {
        var feedback = new EventFeedback
        {
            EventId = eventId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            Status = FeedbackStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.EventFeedback.InsertOneAsync(feedback, cancellationToken: cancellationToken);
        _logger.LogInformation("Feedback created for event {EventId} by user {UserId}", eventId, userId);
        return feedback;
    }

    public async Task<PagedResult<EventFeedback>> ListByEventAsync(string eventId, FeedbackStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var filterBuilder = Builders<EventFeedback>.Filter;
        var filter = filterBuilder.Eq(f => f.EventId, eventId);
        if (status.HasValue)
        {
            filter &= filterBuilder.Eq(f => f.Status, status.Value);
        }

        var total = await _context.EventFeedback.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _context.EventFeedback
            .Find(filter)
            .SortByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EventFeedback>(items, total, page, pageSize);
    }

    public Task<EventFeedback?> GetAsync(string feedbackId, CancellationToken cancellationToken = default)
    {
        return _context.EventFeedback.Find(f => f.Id == feedbackId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EventFeedback?> UpdateAsync(string feedbackId, string userId, UpdateFeedbackDto dto, CancellationToken cancellationToken = default)
    {
        var feedback = await GetAsync(feedbackId, cancellationToken);
        if (feedback is null || feedback.UserId != userId || feedback.Status != FeedbackStatus.Pending)
        {
            return null;
        }

        var update = Builders<EventFeedback>.Update
            .Set(f => f.Rating, dto.Rating)
            .Set(f => f.Comment, dto.Comment)
            .Set(f => f.UpdatedAt, DateTime.UtcNow);

        await _context.EventFeedback.UpdateOneAsync(f => f.Id == feedbackId, update, cancellationToken: cancellationToken);
        feedback.Rating = dto.Rating;
        feedback.Comment = dto.Comment;
        feedback.UpdatedAt = DateTime.UtcNow;
        return feedback;
    }

    public async Task<EventFeedback?> UpdateStatusAsync(string feedbackId, string moderatorId, UpdateFeedbackStatusDto dto, CancellationToken cancellationToken = default)
    {
        var feedback = await GetAsync(feedbackId, cancellationToken);
        if (feedback is null)
        {
            return null;
        }

        var update = Builders<EventFeedback>.Update
            .Set(f => f.Status, dto.Status)
            .Set(f => f.ModeratorId, moderatorId)
            .Set(f => f.ModeratorNote, dto.ModeratorNote)
            .Set(f => f.UpdatedAt, DateTime.UtcNow);

        await _context.EventFeedback.UpdateOneAsync(f => f.Id == feedbackId, update, cancellationToken: cancellationToken);
        feedback.Status = dto.Status;
        feedback.ModeratorId = moderatorId;
        feedback.ModeratorNote = dto.ModeratorNote;
        feedback.UpdatedAt = DateTime.UtcNow;

        if (dto.Status == FeedbackStatus.Approved || dto.Status == FeedbackStatus.Rejected)
        {
            _ = Task.Run(() => RecalculateSummaryAsync(feedback.EventId, CancellationToken.None));
        }

        return feedback;
    }

    public async Task<bool> DeleteAsync(string feedbackId, string requestorId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var feedback = await GetAsync(feedbackId, cancellationToken);
        if (feedback is null)
        {
            return false;
        }

        if (!isAdmin && feedback.UserId != requestorId)
        {
            return false;
        }

        var result = await _context.EventFeedback.DeleteOneAsync(f => f.Id == feedbackId, cancellationToken);
        if (result.DeletedCount > 0)
        {
            _ = Task.Run(() => RecalculateSummaryAsync(feedback.EventId, CancellationToken.None));
            return true;
        }

        return false;
    }

    public async Task<EventRatingSummary> GetSummaryAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var summary = await _context.EventFeedbackSummaries
            .Find(s => s.EventId == eventId)
            .FirstOrDefaultAsync(cancellationToken);

        if (summary != null)
        {
            return summary;
        }

        return await RecalculateSummaryAsync(eventId, cancellationToken);
    }

    private async Task<EventRatingSummary> RecalculateSummaryAsync(string eventId, CancellationToken cancellationToken)
    {
        var filter = Builders<EventFeedback>.Filter.Where(f => f.EventId == eventId && f.Status == FeedbackStatus.Approved);
        var pipeline = await _context.EventFeedback
            .Aggregate()
            .Match(filter)
            .Group(new BsonDocument
            {
                {"_id", "$eventId"},
                {"averageRating", new BsonDocument("$avg", "$rating")},
                {"reviewCount", new BsonDocument("$sum", 1)}
            })
            .FirstOrDefaultAsync(cancellationToken);

        double average = 0;
        long count = 0;

        if (pipeline != null)
        {
            average = pipeline.GetValue("averageRating", 0).ToDouble();
            count = pipeline.GetValue("reviewCount", 0).ToInt64();
        }

        var summary = new EventRatingSummary
        {
            EventId = eventId,
            AverageRating = average,
            ReviewCount = count,
            LastCalculatedAt = DateTime.UtcNow
        };

        await _context.EventFeedbackSummaries.ReplaceOneAsync(
            s => s.EventId == eventId,
            summary,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

        return summary;
    }
}
