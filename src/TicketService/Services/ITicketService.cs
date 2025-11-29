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
    Task<QrVerificationResponse> VerifyQrAsync(QrVerificationRequest request);
    Task<SeatLockResponseDto> LockSeatsAsync(string userId, SeatLockRequestDto dto);
    Task<bool> ReleaseLockAsync(string userId, string lockToken);
    Task<bool> CommitLockAsync(string lockToken, SeatCommitRequestDto dto);
    Task<TicketCancellationResponseDto?> RequestCancellationAsync(int ticketId, string userId, TicketCancellationRequestDto dto);
}

