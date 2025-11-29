using System.ComponentModel.DataAnnotations;
using TicketService.Models;

namespace TicketService.DTOs;

public class SeatLockRequestDto
{
    [Required]
    public string EventId { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<string> SeatCodes { get; set; } = new();

    [Range(60, 900)]
    public int HoldSeconds { get; set; } = 300;
}

public class SeatLockResponseDto
{
    public string LockToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public IReadOnlyCollection<string> SeatCodes { get; set; } = Array.Empty<string>();
}

public class SeatCommitRequestDto
{
    public string? PaymentReference { get; set; }
}
