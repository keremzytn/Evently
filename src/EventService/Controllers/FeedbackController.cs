using EventService.DTOs;
using EventService.Models;
using EventService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers;

[ApiController]
[Route("api/events/{eventId}/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFeedback(string eventId, [FromBody] CreateFeedbackDto dto, CancellationToken cancellationToken)
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunludur" });
        }

        var feedback = await _feedbackService.CreateAsync(eventId, userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetFeedback), new { eventId, feedbackId = feedback.Id }, feedback);
    }

    [HttpGet]
    public async Task<IActionResult> ListFeedback(
        string eventId,
        [FromQuery] FeedbackStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _feedbackService.ListByEventAsync(eventId, status ?? FeedbackStatus.Approved, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{feedbackId}")]
    public async Task<IActionResult> GetFeedback(string eventId, string feedbackId, CancellationToken cancellationToken)
    {
        var feedback = await _feedbackService.GetAsync(feedbackId, cancellationToken);
        if (feedback == null || feedback.EventId != eventId)
        {
            return NotFound();
        }

        return Ok(feedback);
    }

    [HttpGet("average")]
    public async Task<IActionResult> GetAverageRating(string eventId, CancellationToken cancellationToken)
    {
        var summary = await _feedbackService.GetSummaryAsync(eventId, cancellationToken);
        return Ok(summary);
    }

    [HttpPut("~/api/feedback/{feedbackId}")]
    public async Task<IActionResult> UpdateFeedback(string feedbackId, [FromBody] UpdateFeedbackDto dto, CancellationToken cancellationToken)
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunludur" });
        }

        var feedback = await _feedbackService.UpdateAsync(feedbackId, userId, dto, cancellationToken);
        if (feedback == null)
        {
            return NotFound();
        }

        return Ok(feedback);
    }

    [HttpPatch("~/api/feedback/{feedbackId}/status")]
    public async Task<IActionResult> UpdateFeedbackStatus(string feedbackId, [FromBody] UpdateFeedbackStatusDto dto, CancellationToken cancellationToken)
    {
        var moderatorId = Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrWhiteSpace(moderatorId))
        {
            return Unauthorized(new { message = "Moderatör kimliği zorunludur" });
        }

        var feedback = await _feedbackService.UpdateStatusAsync(feedbackId, moderatorId, dto, cancellationToken);
        if (feedback == null)
        {
            return NotFound();
        }

        return Ok(feedback);
    }

    [HttpDelete("~/api/feedback/{feedbackId}")]
    public async Task<IActionResult> DeleteFeedback(string feedbackId, CancellationToken cancellationToken)
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        var isAdmin = Request.Headers["X-User-Role"].ToString().Contains("admin", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Kullanıcı kimliği zorunludur" });
        }

        var result = await _feedbackService.DeleteAsync(feedbackId, userId, isAdmin, cancellationToken);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
