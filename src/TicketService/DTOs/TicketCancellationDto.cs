using System.ComponentModel.DataAnnotations;
using TicketService.Models;

namespace TicketService.DTOs;

public class TicketCancellationRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class TicketCancellationResponseDto
{
    public int RequestId { get; set; }
    public CancellationDecision Decision { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundCurrency { get; set; }
}
