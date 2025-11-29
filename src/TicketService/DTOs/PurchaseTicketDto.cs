using System.ComponentModel.DataAnnotations;

namespace TicketService.DTOs;

public class PurchaseTicketDto
{
    [Required(ErrorMessage = "Etkinlik ID gereklidir")]
    public string EventId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fiyat gereklidir")]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır")]
    public decimal Price { get; set; }

    public string? SeatCode { get; set; }

    public string? SeatLockToken { get; set; }
}

