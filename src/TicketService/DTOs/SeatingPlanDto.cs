using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TicketService.Models;

namespace TicketService.DTOs;

public class SeatingPlanUpsertDto
{
    [Required]
    public string Version { get; set; } = "v1";

    [Required]
    public JsonElement Layout { get; set; }
}

public class SeatingPlanResponseDto
{
    public string EventId { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public JsonElement Layout { get; set; }
}
