using Microsoft.EntityFrameworkCore;
using QRCoder;
using Shared.Events;
using Shared.Kafka;
using TicketService.Data;
using TicketService.DTOs;
using TicketService.Models;

namespace TicketService.Services;

public class TicketServiceImpl : ITicketService
{
    private readonly TicketDbContext _context;
    private readonly KafkaProducer _kafkaProducer;

    public TicketServiceImpl(TicketDbContext context, KafkaProducer kafkaProducer)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Ticket> PurchaseTicketAsync(PurchaseTicketDto dto, string userId)
    {
        var ticketCode = GenerateTicketCode();
        var qrCodeImage = GenerateQRCode(ticketCode);

        var ticket = new Ticket
        {
            EventId = dto.EventId,
            UserId = userId,
            TicketCode = ticketCode,
            Price = dto.Price,
            QRCodeImage = qrCodeImage,
            PurchasedAt = DateTime.UtcNow,
            Status = TicketStatus.Active
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        // Kafka'ya event gönder
        var ticketCreatedEvent = new TicketCreatedEvent
        {
            TicketId = ticket.Id,
            UserId = ticket.UserId,
            EventId = ticket.EventId,
            Price = ticket.Price,
            TicketCode = ticket.TicketCode,
            PurchasedAt = ticket.PurchasedAt
        };

        await _kafkaProducer.ProduceAsync("ticket-created", ticket.TicketCode, ticketCreatedEvent);

        return ticket;
    }

    public async Task<List<Ticket>> GetUserTicketsAsync(string userId)
    {
        return await _context.Tickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.PurchasedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketByCodeAsync(string ticketCode)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
    }

    public async Task<bool> ValidateAndUseTicketAsync(string ticketCode)
    {
        var ticket = await GetTicketByCodeAsync(ticketCode);

        if (ticket == null || ticket.Status != TicketStatus.Active)
            return false;

        ticket.Status = TicketStatus.Used;
        ticket.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> GetTicketQRCodeAsync(string ticketCode)
    {
        var ticket = await GetTicketByCodeAsync(ticketCode);

        if (ticket?.QRCodeImage == null)
            throw new InvalidOperationException("Bilet bulunamadı veya QR kod mevcut değil");

        return ticket.QRCodeImage;
    }

    private string GenerateTicketCode()
    {
        return $"TKT-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
    }

    private byte[] GenerateQRCode(string ticketCode)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(ticketCode, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}

