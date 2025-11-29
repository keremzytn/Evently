using Microsoft.AspNetCore.Mvc;
using TicketService.DTOs;

namespace TicketService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly Services.ITicketService _ticketService;

    public TicketsController(Services.ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketDto dto)
    {
        var userId = Request.Headers["X-User-Id"].ToString();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        try
        {
            var ticket = await _ticketService.PurchaseTicketAsync(dto, userId);
            return Ok(new
            {
                ticketId = ticket.Id,
                ticketCode = ticket.TicketCode,
                eventId = ticket.EventId,
                price = ticket.Price,
                purchasedAt = ticket.PurchasedAt
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = Request.Headers["X-User-Id"].ToString();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });

        var tickets = await _ticketService.GetUserTicketsAsync(userId);
        return Ok(tickets);
    }

    [HttpGet("{ticketCode}")]
    public async Task<IActionResult> GetTicketByCode(string ticketCode)
    {
        var ticket = await _ticketService.GetTicketByCodeAsync(ticketCode);

        if (ticket == null)
            return NotFound(new { message = "Bilet bulunamadı" });

        return Ok(ticket);
    }

    [HttpPost("{ticketCode}/validate")]
    public async Task<IActionResult> ValidateTicket(string ticketCode)
    {
        var result = await _ticketService.ValidateAndUseTicketAsync(ticketCode);

        if (!result)
            return BadRequest(new { message = "Geçersiz bilet veya bilet zaten kullanılmış" });

        return Ok(new { message = "Bilet başarıyla doğrulandı ve kullanıldı" });
    }

    [HttpGet("{ticketCode}/qr")]
    public async Task<IActionResult> GetQRCode(string ticketCode)
    {
        try
        {
            var qrCodeImage = await _ticketService.GetTicketQRCodeAsync(ticketCode);
            return File(qrCodeImage, "image/png");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", service = "Ticket Service", timestamp = DateTime.UtcNow });
    }
}

