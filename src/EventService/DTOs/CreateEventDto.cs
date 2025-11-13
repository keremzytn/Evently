using System.ComponentModel.DataAnnotations;

namespace EventService.DTOs;

public class CreateEventDto
{
    [Required(ErrorMessage = "Başlık gereklidir")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama gereklidir")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konum gereklidir")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Başlangıç tarihi gereklidir")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi gereklidir")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Toplam bilet sayısı gereklidir")]
    [Range(1, int.MaxValue, ErrorMessage = "Bilet sayısı en az 1 olmalıdır")]
    public int TotalTickets { get; set; }

    [Required(ErrorMessage = "Fiyat gereklidir")]
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Kategori gereklidir")]
    public string Category { get; set; } = string.Empty;
}

