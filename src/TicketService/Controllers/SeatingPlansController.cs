using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.DTOs;

namespace TicketService.Controllers;

[ApiController]
[Route("api/events/{eventId}/seating-plan")]
public class SeatingPlansController : ControllerBase
{
    private readonly TicketDbContext _context;

    public SeatingPlansController(TicketDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlan(string eventId)
    {
        var plan = await _context.SeatingPlans.FirstOrDefaultAsync(p => p.EventId == eventId);
        if (plan == null)
        {
            return NotFound(new { message = "Seating plan bulunamadı" });
        }

        var layout = JsonDocument.Parse(plan.LayoutJson).RootElement.Clone();
        var response = new SeatingPlanResponseDto
        {
            EventId = plan.EventId,
            Version = plan.Version,
            Layout = layout
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpsertPlan(string eventId, [FromBody] SeatingPlanUpsertDto dto)
    {
        var plan = await _context.SeatingPlans.FirstOrDefaultAsync(p => p.EventId == eventId);
        var layoutJson = dto.Layout.GetRawText();

        if (plan == null)
        {
            plan = new TicketService.Models.SeatingPlan
            {
                EventId = eventId,
                Version = dto.Version,
                LayoutJson = layoutJson,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SeatingPlans.Add(plan);
        }
        else
        {
            plan.Version = dto.Version;
            plan.LayoutJson = layoutJson;
            plan.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Seating plan kaydedildi" });
    }
}
