using TicketService.DTOs;
using TicketService.Models;

namespace TicketService.Services;

public interface ITicketService
{
    Task<Ticket> PurchaseTicketAsync(PurchaseTicketDto dto, string userId);
    Task<List<Ticket>> GetUserTicketsAsync(string userId);
    Task<Ticket?> GetTicketByCodeAsync(string ticketCode);
    Task<bool> ValidateAndUseTicketAsync(string ticketCode);
    Task<byte[]> GetTicketQRCodeAsync(string ticketCode);
}

