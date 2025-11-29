using System.ComponentModel.DataAnnotations;
using TicketService.Models;

namespace TicketService.DTOs;

public class QrVerificationRequest
{
    [Required]
    public string Payload { get; set; } = string.Empty;

    public string? DeviceId { get; set; }
}

public class QrVerificationResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TicketCode { get; set; }
    public TicketStatus? TicketStatus { get; set; }
}
